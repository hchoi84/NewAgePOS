using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChannelAdvisorLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOS.ViewModels;
using NewAgePOSModels.Models;
using Newtonsoft.Json.Linq;
using SkuVaultLibrary;

namespace NewAgePOS.Pages.Product
{
  public class SearchModel : PageModel
  {
    private readonly IChannelAdvisor _ca;
    private readonly ISkuVault _sv;

    public SearchModel(IChannelAdvisor ca, ISkuVault sv)
    {
      _ca = ca;
      _sv = sv;
    }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "SKU or UPC")]
    public string Codes { get; set; }

    public List<LocationSearchViewModel> Results { get; set; }

    public async Task<IActionResult> OnGet()
    {
      if (string.IsNullOrEmpty(Codes)) return Page();

      Results = new List<LocationSearchViewModel>();

      IEnumerable<string> productCodes = Codes.Trim().Replace(" ", string.Empty).Split(Environment.NewLine).Distinct();

      await AddProducts(productCodes);

      if (Results.Count > 0)
        await AddLocationsAsync();

      Results = Results.Where(r => r.Locations != null && r.Locations.Any())
        .OrderBy(r => r.Sku)
        .ToList();

      return Page();
    }

    private async Task AddProducts(IEnumerable<string> productCodes)
    {
      List<JObject> jObjects = await _ca.GetProductsByCodeAsync(productCodes.ToList());

      foreach (var item in jObjects)
      {
        if (string.IsNullOrEmpty(item[CAStrings.whLoc].ToString()) ||
          item[CAStrings.whLoc].ToString() == "DROPSHIP(19999)") continue;

        Results.Add(new LocationSearchViewModel
        {
          Sku = item[CAStrings.sku].ToString(),
          Upc = item[CAStrings.upc].ToString(),
          Cost = String.IsNullOrEmpty(item[CAStrings.cost].ToString()) ? 0 :
          item[CAStrings.cost].ToObject<float>(),
          Price = item[CAStrings.attributes]
          .FirstOrDefault(i => i[CAStrings.name]
            .ToString() == CAStrings.bcprice)[CAStrings.Value]
          .ToObject<float>(),
          AllName = item[CAStrings.attributes]
          .FirstOrDefault(i => i[CAStrings.name].ToString() == CAStrings.allName)[CAStrings.Value]
          .ToString()
        });
      }
    }

    private async Task AddLocationsAsync()
    {
      List<string> skus = Results.Select(p => p.Sku).ToList();
      JObject result = await _sv.GetInventoryLocationsAsync(skus, true);
      //JToken items = result["Items"];
      var items = result["Items"].Select(i => i.ToObject<JProperty>());

      if (items == null) return;

      foreach (var r in Results)
      {
        var item = items.FirstOrDefault(j => j.Name == r.Sku || j.Name == r.Upc);

        if (item == null) continue;
        if (item.Value.Count() == 0) continue;
        if (item.Value.Where(v => v["LocationCode"].ToString() != "DROPSHIP").Count() == 0) continue;

        r.Locations.AddRange(item.Value
          .OrderBy(v => v["LocationCode"].ToString())
          .Select(v =>
            {
              return new ProductLocationModel
              {
                Code = item.Name,
                Location = v["LocationCode"].ToString(),
                Qty = v["Quantity"].ToObject<int>()
              };
            }));
      }
    }

    public async Task<IActionResult> OnPostTransferAsync(string code, string location, int qty, int transferqty)
    {
      if (transferqty > qty)
      {
        ModelState.AddModelError(string.Empty, "Can not transfer more than available quantity");
        return Page();
      }

      List<AddRemoveItemBulkModel> itemsToTransfer = new List<AddRemoveItemBulkModel>
      {
        new AddRemoveItemBulkModel
        {
          Code = code,
          LocationCode = location,
          Quantity = qty,
          Reason = "Transfer"
        }
      };

      JObject result = await _sv.RemoveItemBulkAsync(itemsToTransfer);

      List<string> messages = new List<string>() { "Transfer complete" };

      if (result["Errors"].ToObject<JArray>().Any())
      {
        foreach (var e in (JArray)result["Errors"])
        {
          messages.Add($"{ e["Sku"] ?? e["Code"] }: { e["ErrorMessages"][0] } { e["LocationCode"] }");
        }
      }

      itemsToTransfer.ForEach(i => i.LocationCode = "STORE");

      result = await _sv.AddItemBulkAsync(itemsToTransfer);

      if (result["Errors"].ToObject<JArray>().Any())
      {
        foreach (var e in (JArray)result["Errors"])
        {
          messages.Add($"{ e["Sku"] ?? e["Code"] }: { e["ErrorMessages"][0] } { e["LocationCode"] }");
        }
      }

      TempData["Message"] = string.Join(Environment.NewLine, messages);

      Thread.Sleep(1000);

      return RedirectToPage(new { Codes });
    }
  }
}
