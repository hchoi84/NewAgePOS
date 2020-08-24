using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;
using NewAgePOSLibrary.Models;
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

    [BindProperty(SupportsGet = true)]
    public List<SaleLineModel> SaleLines { get; set; }

    [BindProperty(SupportsGet = true)]
    public float TaxPct { get; set; }

    [BindProperty(SupportsGet = true)]
    public CustomerModel Customer { get; set; }

    [BindProperty]
    public TransactionModel Transaction { get; set; }

    public void OnGet()
    {
      bool isComplete = _sqlDb.Sales_GetStatus(SaleId);
      if (isComplete)
      {
        TempData["Message"] = $"Cannot access Checkout because Sale Id { SaleId } was completed.";
        RedirectToPage("Index");
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

    public IActionResult OnPost()
    {
      // Create Transaction
      _sqlDb.Transactions_Insert(SaleId, Transaction.Amount, Transaction.PaymentType, "Checkout", Transaction.Message);

      // Mark Sale as Complete
      _sqlDb.Sales_MarkComplete(SaleId);

      // Remove product(s) from SkuVault
      // TODO: Test what'll happen if SkuVault doesn't have the quantity to remove
      Dictionary<string, int> productsToRemove = new Dictionary<string, int>();
      SaleLines.ForEach(s => productsToRemove.Add(s.Sku, s.Qty));
      _skuVault.RemoveProducts(productsToRemove);

      // Redirect to Receipt page
      return RedirectToPage("Receipt", new { SaleId });
    }
  }
}
