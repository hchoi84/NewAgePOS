using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;
using Newtonsoft.Json.Linq;
using SkuVaultLibrary;

namespace NewAgePOS.Pages.Sale
{
  public class CheckoutModel : PageModel
  {
    private readonly ISQLData _sqlDb;
    private readonly ISkuVault _skuVault;

    public CheckoutModel(ISQLData sqlDb, ISkuVault skuVault)
    {
      _sqlDb = sqlDb;
      _skuVault = skuVault;
    }

    [BindProperty(SupportsGet = true)]
    public int SaleId { get; set; }

    public List<SaleLineModel> SaleLines { get; set; }

    public float TaxPct { get; set; }

    public CustomerModel Customer { get; set; }

    [BindProperty]
    public TransactionModel Transaction { get; set; }

    public void OnGet()
    {
      bool isComplete = _sqlDb.Sales_GetStatus(SaleId);
      if (isComplete)
      {
        TempData["Message"] = $"Cannot access Checkout because Sale Id { SaleId } was completed.";
        RedirectToPage("Search");
      }

      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      TaxPct = _sqlDb.Taxes_GetBySaleId(SaleId);

      Customer = _sqlDb.Customers_GetBySaleId(SaleId);
      if (Customer != null)
      {
        TextInfo ti = new CultureInfo("en-US", false).TextInfo;

        Customer.FirstName = ti.ToTitleCase(Customer.FirstName);
        Customer.LastName = ti.ToTitleCase(Customer.LastName);
      }
    }

    public async Task<IActionResult> OnPost()
    {
      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      List<AddRemoveItemBulkModel> itemsToRemove = new List<AddRemoveItemBulkModel>();

      itemsToRemove = SaleLines.Select(s =>
      new AddRemoveItemBulkModel
      {
        Code = s.Sku,
        LocationCode = "STORE",
        Quantity = s.Qty,
        Reason = "Store Sale"
      }).ToList();

      JObject result = await _skuVault.RemoveItemBulkAsync(itemsToRemove);
      List<string> errorMsgs = new List<string>();

      if (result["Errors"].ToObject<JArray>().Any())
      {
        foreach (var e in (JArray)result["Errors"])
        {
          errorMsgs.Add($"{ e["Sku"] }: { e["ErrorMessages"][0] } { e["LocationCode"] }");
        }
      }

      TempData["Message"] = string.Join(Environment.NewLine, errorMsgs);

      _sqlDb.Transactions_Insert(SaleId, Transaction.Amount, Transaction.Method, "Checkout", Transaction.Message);

      _sqlDb.Sales_MarkComplete(SaleId);

      return RedirectToPage("Receipt", new { SaleId });
    }
  }
}
