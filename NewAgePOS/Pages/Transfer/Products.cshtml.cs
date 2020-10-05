using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChannelAdvisorLibrary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NewAgePOS.Utilities;
using NewAgePOS.ViewModels;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;
using Newtonsoft.Json.Linq;
using SkuVaultLibrary;

namespace NewAgePOS.Pages.Transfer
{
  public class ProductsModel : PageModel
  {
    private readonly IChannelAdvisor _ca;
    private readonly ISkuVault _sv;
    private readonly ISQLData _sqlDb;

    public ProductsModel(IChannelAdvisor ca, ISkuVault sv, ISQLData sqlDb)
    {
      _ca = ca;
      _sv = sv;
      _sqlDb = sqlDb;
    }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "SKU or UPC")]
    public string Codes { get; set; }

    [BindProperty(SupportsGet = true)]
    public TransferPageTypeEnum PageType { get; set; }

    [BindProperty]
    public List<TransferRequestViewModel> TransferRequests { get; set; }

    [BindProperty(SupportsGet = true)]
    public int TransferRequestId { get; set; }

    public List<LocationSearchViewModel> ViewModels { get; set; }
    public int XferBatchReqCount { get; set; }
    public List<SelectListItem> PendingTransfers { get; set; }
    public bool IsReview { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
      if (string.IsNullOrEmpty(Codes)) return Page();
      
      await InitializeAsync();

      if (PageType == TransferPageTypeEnum.Batch)
      {
        InitializeBatch();
      }

      return Page();
    }

    private async Task InitializeAsync()
    {
      ViewModels = new List<LocationSearchViewModel>();
      IEnumerable<string> productCodes = Codes.CountIt().Select(c => c.Key);

      await AddProducts(productCodes);
      if (ViewModels.Count == 0)
      {
        TempData["Message"] = "No Results Found";
        return;
      }

      await AddLocationsAsync();

      ViewModels = ViewModels.Where(r => r.Locations.Any())
        .OrderBy(r => r.Sku)
        .ToList();
    }

    private async Task AddProducts(IEnumerable<string> productCodes)
    {
      IEnumerable<JObject> jObjects = await _ca.GetProductsByCodeAsync(productCodes);

      foreach (var item in jObjects)
      {
        if (string.IsNullOrEmpty(item[CAStrings.whLoc].ToString())
          || item[CAStrings.whLoc].ToString() == "DROPSHIP(19999)"
          || item[CAStrings.whLoc].ToString() == "Out of Stock(0)") continue;

        ViewModels.Add(new LocationSearchViewModel
        {
          Sku = item[CAStrings.sku].ToString(),
          Upc = item[CAStrings.upc].ToString(),
          Cost = string.IsNullOrEmpty(item[CAStrings.cost].ToString()) ? 0 :
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
      List<string> skus = ViewModels.Select(p => p.Sku).ToList();
      JObject result = await _sv.GetInventoryLocationsAsync(skus, true);
      IEnumerable<JProperty> items = result["Items"].Select(i => i.ToObject<JProperty>());

      if (items == null) return;

      foreach (var m in ViewModels)
      {
        var item = items.FirstOrDefault(j => j.Name == m.Sku || j.Name == m.Upc);

        if (item == null) continue;
        if (item.Value.Count() == 0) continue;

        foreach (JObject loc in item.Value)
        {
          if (loc["LocationCode"].ToString() == "DROPSHIP") continue;

          m.Locations.Add(new ProductLocationModel
          {
            Code = item.Name,
            Location = loc["LocationCode"].ToString(),
            Qty = loc["Quantity"].ToObject<int>()
          });
        }

        m.Locations = m.Locations.OrderBy(l => l.Location).ToList();
      }
    }

    private void InitializeBatch()
    {
      TransferRequests = new List<TransferRequestViewModel>();
      PendingTransfers = new List<SelectListItem>();

      ViewModels.ForEach(vm => TransferRequests.Add(
        new TransferRequestViewModel
        {
          Sku = vm.Sku
        }));

      IEnumerable<TransferRequestModel> transferRequests = _sqlDb.TransferRequests_GetByStatus(StatusEnum.Pending);
      IEnumerable<TransferRequestItemModel> transferRequestItems = _sqlDb.TransferRequestItems_GetByStatus(StatusEnum.Pending);
      foreach (var tr in transferRequests)
      {
        int itemCount = transferRequestItems.Where(tri => tri.TransferRequestId == tr.Id).Count();
        PendingTransfers.Add(new SelectListItem
        {
          Text = $"{tr.Description} ({itemCount} items)",
          Value = tr.Id.ToString()
        });
      }
    }


    public async Task<IActionResult> OnPostTransferAsync(string code, string location, int qty, int transferqty)
    {
      if (transferqty <= 0)
      {
        TempData["Message"] = $"{ code } { location }: Transfer quantity must be greater than 0";
        return RedirectToPage(new { Codes });
      }

      if (transferqty > qty)
      {
        TempData["Message"] = $"{ code } { location }: Can not transfer more than available ({ transferqty } / { qty })";
        return RedirectToPage(new { Codes });
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

      return RedirectToPage();
    }


    public IActionResult OnPostAddItemsToTransferRequest()
    {
      if (TransferRequestId == 0)
      {
        TempData["Message"] = "Please choose Transfer";
        return RedirectToPage(new { Codes });
      }

      IEnumerable<TransferRequestViewModel> requestItems = TransferRequests.Where(tr => tr.Qty > 0);
      if (!requestItems.Any())
      {
        TempData["Message"] = "No items have been selected";
        return RedirectToPage(new { Codes });
      }

      List<string> errorMsgs = new List<string>();
      errorMsgs.Add("Products added successfully");
      IEnumerable<TransferRequestItemModel> transferRequestItems = _sqlDb.TransferRequestItems_GetByTransferRequestId(TransferRequestId);
      foreach (var ri in requestItems)
      {
        bool isFound = transferRequestItems.FirstOrDefault(tri => tri.Sku == ri.Sku) != null;
        if (isFound)
        {
          errorMsgs.Add($"{ ri.Sku } already exists in Transfer");
          continue;
        }

        _sqlDb.TransferRequestItems_Insert(TransferRequestId, ri.Sku, ri.Qty);

      }

      if (errorMsgs.Count > 0)
      {
        TempData["Message"] = string.Join(Environment.NewLine, errorMsgs);
      }

      return RedirectToPage(new { TransferRequestId });
    }

    public IActionResult OnPostCreateTransferRequestAndAddItems(string description, string creatorName)
    {
      if (string.IsNullOrEmpty(description) || string.IsNullOrEmpty(creatorName))
      {
        TempData["Message"] = "Description and Name fields are required";
        return RedirectToPage(new { Codes });
      }

      IEnumerable<TransferRequestViewModel> requestItems = TransferRequests.Where(tr => tr.Qty > 0);
      if (!requestItems.Any())
      {
        TempData["Message"] = "No items have been selected";
        return RedirectToPage(new { Codes });
      }

      TransferRequestId = _sqlDb.TransferRequests_Insert(description, creatorName);
      foreach (var ri in requestItems)
      {
        _sqlDb.TransferRequestItems_Insert(TransferRequestId, ri.Sku, ri.Qty);
      }

      return RedirectToPage();
    }


    public IActionResult OnGetDisplayXferReqItems()
    {
      IsReview = true;
      List<LocationSearchViewModel> xferReqItems = HttpContext.Session.GetObject<List<LocationSearchViewModel>>("XferReqItems");

      if (xferReqItems == null)
      {
        TempData["Message"] = "There are no items";
        return Page();
      }

      XferBatchReqCount = xferReqItems.Count;
      ViewModels = xferReqItems;

      return Page();
    }

    public IActionResult OnPostEditXferReqItems()
    {
      IsReview = true;
      List<LocationSearchViewModel> xferReqItems = HttpContext.Session.GetObject<List<LocationSearchViewModel>>("XferReqItems");

      foreach (var r in ViewModels)
      {
        LocationSearchViewModel xferReqItem = xferReqItems.FirstOrDefault(x => x.Sku == r.Sku);

        if (r.RequestQty == 0) xferReqItems.Remove(xferReqItem);
        else if (r.RequestQty != xferReqItem.RequestQty) xferReqItem.RequestQty = r.RequestQty;
      }

      HttpContext.Session.SetObject("XferReqItems", xferReqItems);

      return RedirectToPage("/Product/Transfer", "DisplayXferReqItems", new { IsReview, PageType });
    }
  }
}
