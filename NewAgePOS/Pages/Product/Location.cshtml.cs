using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSModels.Models;
using Newtonsoft.Json.Linq;
using SkuVaultLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NewAgePOS.Pages.Product
{
  public class LocationModel : PageModel
  {
    private readonly ISkuVault _skuVault;

    public LocationModel(ISkuVault skuVault)
    {
      _skuVault = skuVault;
    }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "SKUs or UPCs")]
    public string Codes { get; set; }

    public List<ProductLocationModel> Locations { get; set; } = new List<ProductLocationModel>();

    public async Task<IActionResult> OnGet()
    {
      if (string.IsNullOrEmpty(Codes)) return Page();

      IEnumerable<string> productCodes = Codes.Trim().Replace(" ", string.Empty).Split(Environment.NewLine).Distinct();

      List<string> skus = new List<string>();
      List<string> upcs = new List<string>();

      foreach (var code in productCodes)
      {
        if (code.Contains("_"))
          skus.Add(code);
        else
          upcs.Add(code);
      }

      if (skus.Count > 0)
      {
        JObject result = await _skuVault.GetInventoryLocationsAsync(skus, true);
        AddToLocations(result);
      }

      if (upcs.Count > 0)
      {
        JObject result = await _skuVault.GetInventoryLocationsAsync(upcs, false);
        AddToLocations(result);
      }

      return Page();
    }

    private void AddToLocations(JObject result)
    {
      JToken items = result["Items"];

      if (items != null)
      {
        foreach (JProperty item in items)
        {
          foreach (JObject i in item.Value)
          {
            if (i["LocationCode"].ToString() == "DROPSHIP") continue;

            Locations.Add(new ProductLocationModel
            {
              Code = item.Name,
              Location = i["LocationCode"].ToString(),
              Qty = i["Quantity"].ToObject<int>()
            });
          }
        }
      }
    }

    public async Task<IActionResult> OnPostRemoveAsync(string code, string location, int qty, int removeqty)
    {
      if (removeqty > qty)
      {
        ModelState.AddModelError(string.Empty, "Can not remove more than available quantity");
        return Page();
      }

      List<AddRemoveItemBulkModel> itemsToRemove = new List<AddRemoveItemBulkModel>
      {
        new AddRemoveItemBulkModel
        {
          Code = code,
          LocationCode = location,
          Quantity = qty,
          Reason = "Remove"
        }
      };

      JObject result = await _skuVault.RemoveItemBulkAsync(itemsToRemove);

      List<string> messages = new List<string>() { "Removal complete" };

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

      JObject result = await _skuVault.RemoveItemBulkAsync(itemsToTransfer);

      List<string> messages = new List<string>() { "Transfer complete" };

      if (result["Errors"].ToObject<JArray>().Any())
      {
        foreach (var e in (JArray)result["Errors"])
        {
          messages.Add($"{ e["Sku"] ?? e["Code"] }: { e["ErrorMessages"][0] } { e["LocationCode"] }");
        }
      }

      itemsToTransfer.ForEach(i => i.LocationCode = "STORE");

      result = await _skuVault.AddItemBulkAsync(itemsToTransfer);

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
