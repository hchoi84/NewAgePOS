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
    public SaleTransactionModel SaleTransaction { get; set; }

    public void OnGet()
    {
      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      TaxPct = _sqlDb.Taxes_GetBySaleId(SaleId);

      SaleLines.ForEach(s => s.LineTotal = (s.Price - s.DiscAmt) * (1 - s.DiscPct / 100) * s.Qty);

      Customer = _sqlDb.Customers_GetBySaleId(SaleId);
      if (Customer != null)
      {
        TextInfo ti = new CultureInfo("en-US", false).TextInfo;

        Customer.FirstName = ti.ToTitleCase(Customer.FirstName);
        Customer.LastName = ti.ToTitleCase(Customer.LastName);
      }
    }

    public void OnPost()
    {
      // Create SaleTransaction
      _sqlDb.SaleTransaction_Insert(SaleId, SaleTransaction.Amount, SaleTransaction.PaymentType, "Checkout", SaleTransaction.Message);

      // Mark Sale as Complete

      // Remove product(s) from SkuVault
      // TODO: Test what'll happen if SkuVault doesn't have the quantity to remove
      //Dictionary<string, int> productsToRemove = new Dictionary<string, int>();
      //SaleLines.ForEach(s => productsToRemove.Add(s.Sku, s.Qty));
      //_skuVault.RemoveProducts(productsToRemove);

      // Calculate change. Move this to the Receipt page
      float change = SaleTransaction.Amount - SaleLines.Sum(s => s.LineTotal);

      // Redirect to Receipt page
    }
  }
}
