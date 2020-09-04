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

    [BindProperty]
    public TransactionModel Transaction { get; set; }

    public List<SaleLineModel> SaleLines { get; set; }
    public List<ProductModel> Products { get; set; } = new List<ProductModel>();
    public List<GiftCardModel> GiftCards { get; set; } = new List<GiftCardModel>();
    public float TaxPct { get; set; }
    public CustomerModel Customer { get; set; }

    public IActionResult OnGet()
    {
      bool isComplete = _sqlDb.Sales_GetById(SaleId).IsComplete;
      if (isComplete)
      {
        TempData["Message"] = $"Cannot access Checkout because Sale Id { SaleId } was completed.";
        return RedirectToPage("Search");
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

      foreach (SaleLineModel saleLine in SaleLines)
      {
        if (saleLine.ProductId.HasValue) Products.Add(_sqlDb.Products_GetById(saleLine.ProductId.Value));
        if (saleLine.GiftCardId.HasValue) GiftCards.Add(_sqlDb.GiftCards_GetById(saleLine.GiftCardId.Value));
      }

      return Page();
    }

    public async Task<IActionResult> OnPost()
    {
      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId).Where(s => s.ProductId != null).ToList();
      List<AddRemoveItemBulkModel> itemsToRemove = new List<AddRemoveItemBulkModel>();

      itemsToRemove = SaleLines.Select(s =>
      new AddRemoveItemBulkModel
      {
        Code = _sqlDb.Products_GetById(s.ProductId.Value).Sku,
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

      _sqlDb.Transactions_Insert(SaleId, null, Transaction.Amount, Transaction.Method, "Checkout", Transaction.Message);

      _sqlDb.Sales_MarkComplete(SaleId);

      return RedirectToPage("Receipt", new { SaleId });
    }
  }
}
