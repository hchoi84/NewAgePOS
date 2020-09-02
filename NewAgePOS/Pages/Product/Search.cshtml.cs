using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChannelAdvisorLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;
using Newtonsoft.Json.Linq;
using SkuVaultLibrary;

namespace NewAgePOS.Pages.Product
{
  public class SearchModel : PageModel
  {
    private readonly ISQLData _sqlDb;
    private readonly IChannelAdvisor _ca;
    private readonly ISkuVault _sv;

    public SearchModel(ISQLData sqlDb, IChannelAdvisor ca, ISkuVault sv)
    {
      _sqlDb = sqlDb;
      _ca = ca;
      _sv = sv;
    }

    private readonly string _sku = "Sku";
    private readonly string _upc = "UPC";
    private readonly string _allName = "All Name";
    private readonly string _attributes = "Attributes";
    private readonly string _name = "Name";
    private readonly string _Value = "Value";
    private readonly string _whLoc = "WarehouseLocation";

    [BindProperty(SupportsGet = true)]
    [Display(Name = "SKU or UPC")]
    public string Codes { get; set; }

    public List<ProductSearchModel> Products { get; set; } = new List<ProductSearchModel>();

    public async Task<IActionResult> OnGet()
    {
      if (string.IsNullOrEmpty(Codes)) return Page();

      IEnumerable<string> productCodes = Codes.Trim().Replace(" ", string.Empty).Split(Environment.NewLine).Distinct();
    
      await AddProducts(productCodes);

      await AddLocationsAsync();

      Products.Where(p => p.Location.Count > 0);

      return Page();
    }

    private async Task AddProducts(IEnumerable<string> productCodes)
    {
      List<JObject> jObjects = await _ca.GetProductsByCodeAsync(productCodes.ToList());

      foreach (var item in jObjects)
      {
        if (string.IsNullOrEmpty(item[_whLoc].ToString()) ||
          item[_whLoc].ToString() == "DROPSHIP(19999)") continue;

        Products.Add(new ProductSearchModel
        {
          Sku = item[_sku].ToString(),
          Upc = item[_upc].ToString(),
          AllName = item[_attributes]
          .FirstOrDefault(i => i[_name].ToString() == _allName)[_Value]
          .ToString()
        });
      }
    }

    private async Task AddLocationsAsync()
    {
      List<string> skus = Products.Select(p => p.Sku).ToList();
      JObject result = await _sv.GetInventoryLocationsAsync(skus, true);
      JToken items = result["Items"];

      if (items != null)
      {
        foreach (JProperty item in items)
        {
          foreach (JObject i in item.Value)
          {
            if (i["LocationCode"].ToString() == "DROPSHIP") continue;

            ProductSearchModel p = Products.FirstOrDefault(p => p.Sku == item.Name);
            string location = i["LocationCode"].ToString();
            int qty = i["Quantity"].ToObject<int>();
            p.Location.Add(location, qty);
          }
        }
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
