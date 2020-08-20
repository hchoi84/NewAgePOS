using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;
using NewAgePOSLibrary.Models;

namespace NewAgePOS.Pages.Sale
{
  public class ReceiptModel : PageModel
  {
    private readonly ISQLData _sqlDb;

    public ReceiptModel(ISQLData _sqlDb)
    {
      this._sqlDb = _sqlDb;
    }

    [BindProperty(SupportsGet = true)]
    public int SaleId { get; set; }

    [BindProperty(SupportsGet = true)]
    public SaleModel Sale { get; set; }

    [BindProperty(SupportsGet = true)]
    public List<SaleLineModel> SaleLines { get; set; }

    [BindProperty(SupportsGet = true)]
    public float TaxPct { get; set; }

    [BindProperty(SupportsGet = true)]
    public string FullName { get; set; }

    [BindProperty(SupportsGet = true)]
    public List<TransactionModel> Transactions { get; set; }

    public void OnGet()
    {
      Sale = _sqlDb.Sales_GetById(SaleId);
      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      TaxPct = _sqlDb.Taxes_GetBySaleId(SaleId);
      FullName = _sqlDb.Customers_GetBySaleId(SaleId).FirstName;
      Transactions = _sqlDb.Transactions_GetBySaleId(SaleId);
    }
  }
}
