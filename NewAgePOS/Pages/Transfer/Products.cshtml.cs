using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
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

    [BindProperty(SupportsGet = true)]
    public int TransferRequestId { get; set; }

    [BindProperty]
    public List<TransferRequestViewModel> TransferRequestQtys { get; set; }

    public TransferRequestModel TransferRequest { get; set; }
    public List<LocationSearchViewModel> ViewModels { get; set; }
    public List<SelectListItem> PendingTransferRequests { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
      if (PageType == TransferPageTypeEnum.Single || PageType == TransferPageTypeEnum.Batch)
      {
        if (string.IsNullOrEmpty(Codes)) return Page();
        await InitializeAsync();
      }

      if (PageType == TransferPageTypeEnum.Batch)
        InitializeBatch();

      if (PageType == TransferPageTypeEnum.Review)
      {
        await InitializeReviewAsync();
        if (TransferRequest == null)
        {
          TempData["Message"] = "No transfer request found with the given Id";
          return RedirectToPage("/Transfer/Search");
        }
      }

      return Page();
    }

    private async Task InitializeAsync()
    {
      ViewModels = new List<LocationSearchViewModel>();
      IEnumerable<string> productCodes = Codes.CountIt().Select(c => c.Key);

      await AddProductsAsync(productCodes);
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

    private async Task AddProductsAsync(IEnumerable<string> productCodes)
    {
      IEnumerable<JObject> jObjects = await _ca.GetProductsByCodeAsync(productCodes);

      foreach (var item in jObjects)
      {
        if (string.IsNullOrEmpty(item[CAStrings.whLoc].ToString())
          || item[CAStrings.whLoc].ToString() == "DROPSHIP(19999)"
          || item[CAStrings.whLoc].ToString() == "Out of Stock(0)") 
          continue;

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
      TransferRequestQtys = new List<TransferRequestViewModel>();

      ViewModels.ForEach(vm => TransferRequestQtys.Add(
        new TransferRequestViewModel
        {
          Sku = vm.Sku
        }));

      PendingTransferRequests = new List<SelectListItem>();
      TransferRequestModel defaultTransferRequest = _sqlDb.TransferRequests_GetDefault();
      int itemCount, id;
      string description;
      if (defaultTransferRequest == null)
      {
        id = _sqlDb.TransferRequests_Insert("Default", "Store");
        itemCount = 0;
        description = "Default";
      }
      else
      {
        id = defaultTransferRequest.Id;
        itemCount = _sqlDb.TransferRequestItems_GetByTransferRequestId(defaultTransferRequest.Id).Count();
        description = defaultTransferRequest.Description;
      }

      TransferRequestId = id;
      PendingTransferRequests.Add(new SelectListItem
      {
        Text = $"{description} ({itemCount} items)",
        Value = id.ToString()
      });

      //List<TransferRequestModel> pendingTransferRequests = _sqlDb.TransferRequests_GetByStatus(StatusEnum.Pending);
      //foreach (var tr in pendingTransferRequests)
      //{
      //  int itemCount = _sqlDb.TransferRequestItems_GetByTransferRequestId(tr.Id).Count();
      //  PendingTransferRequests.Add(new SelectListItem
      //  {
      //    Text = $"{tr.Description} ({itemCount} items)",
      //    Value = tr.Id.ToString()
      //  });
      //}
    }

    private async Task InitializeReviewAsync()
    {
      ViewModels = new List<LocationSearchViewModel>();
      TransferRequest = _sqlDb.TransferRequests_GetDefault();
      TransferRequestId = TransferRequest.Id;
      //TransferRequest = _sqlDb.TransferRequests_GetById(TransferRequestId);
      if (TransferRequest == null) return;

      TransferRequestQtys = new List<TransferRequestViewModel>();

      IEnumerable<TransferRequestItemModel> tris = _sqlDb.TransferRequestItems_GetByTransferRequestId(TransferRequestId);
      foreach (var tri in tris)
      {
        TransferRequestQtys.Add(new TransferRequestViewModel
        {
          Sku = tri.Sku,
          Qty = tri.Qty
        });
      }

      IEnumerable<string> skus = tris.Select(tri => tri.Sku);
      //TransferRequest.Description += $" ({ skus.Count() } items)";

      await AddProductsAsync(skus);
      await AddLocationsAsync();
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
          Quantity = transferqty,
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
        TempData["Message"] = "Please choose a Transfer Request";
        return RedirectToPage(new { Codes });
      }

      IEnumerable<TransferRequestViewModel> requestItems = TransferRequestQtys.Where(tr => tr.Qty > 0);
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

    //public IActionResult OnPostCreateTransferRequestAndAddItems(string description, string creatorName)
    //{
    //  if (string.IsNullOrEmpty(description) || string.IsNullOrEmpty(creatorName))
    //  {
    //    TempData["Message"] = "Description and Name fields are required";
    //    return RedirectToPage(new { Codes });
    //  }

    //  if (description.Length > 20 || creatorName.Length > 10)
    //  {
    //    TempData["Message"] = "Description or Name fields character length is too long";
    //    return RedirectToPage(new { Codes });
    //  }

    //  IEnumerable<TransferRequestViewModel> requestItems = TransferRequestQtys.Where(tr => tr.Qty > 0);
    //  if (!requestItems.Any())
    //  {
    //    TempData["Message"] = "No items have been selected";
    //    return RedirectToPage(new { Codes });
    //  }

    //  TransferRequestId = _sqlDb.TransferRequests_Insert(description, creatorName);
    //  foreach (var ri in requestItems)
    //  {
    //    _sqlDb.TransferRequestItems_Insert(TransferRequestId, ri.Sku, ri.Qty);
    //  }

    //  return RedirectToPage();
    //}


    public IActionResult OnPostEditTransferQtys()
    {
      if (TransferRequestId == 0)
      {
        TransferRequestId = _sqlDb.TransferRequests_GetDefault().Id;
      }

      IEnumerable<TransferRequestItemModel> tris = _sqlDb.TransferRequestItems_GetByTransferRequestId(TransferRequestId);
      int updatedCount = 0;

      foreach (var trq in TransferRequestQtys)
      {
        var tri = tris.FirstOrDefault(tri => tri.Sku == trq.Sku);
        if (trq.Qty <= 0)
        {
          updatedCount++;
          _sqlDb.TransferRequestItems_Delete(tri.Id);
        }
        else if (tri.Qty != trq.Qty)
        {
          updatedCount++;
          _sqlDb.TransferRequestItems_Update(tri.Id, trq.Qty);
        }
      }

      if (updatedCount > 0)
        TempData["Message"] = $"{ updatedCount } product(s) updated";
      else
        TempData["Message"] = "Nothing to update";

      return RedirectToPage();
    }

    public IActionResult OnPostClearItems(int id)
    {
      _sqlDb.TransferRequestItems_ClearByTransferRequestId(id);
      return RedirectToPage();
    }
  }
}
