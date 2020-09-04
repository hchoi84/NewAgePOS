using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;

namespace NewAgePOS.Pages.Sale
{
  public class ReceiptModel : PageModel
  {
    private readonly ISQLData _sqlDb;

    public ReceiptModel(ISQLData _sqlDb)
    {
      this._sqlDb = _sqlDb;
    }

    public string FullName { get; set; }
    public float Subtotal { get; set; }
    public float Discount { get; set; }
    public float TaxPct { get; set; }
    public float Tax { get; set; }
    public float Total { get; set; }
    public float Change { get; set; }

    [BindProperty(SupportsGet = true)]
    public int SaleId { get; set; }

    [Display(Name = "Sale Date")]
    public string Created { get; set; }

    [Display(Name = "Total Quantity")]
    public int TotalQty { get; set; }

    public List<TransactionModel> Transactions { get; set; }

    public void OnGet()
    {
      FullName = _sqlDb.Customers_GetBySaleId(SaleId).FullName;
      Created = _sqlDb.Sales_GetById(SaleId).Created.ToString("yyyy/MM/dd");

      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      TotalQty = saleLines.Sum(s => s.Qty);
      Subtotal = saleLines.Sum(sl => sl.LineTotal);
      Discount = saleLines.Sum(sl => sl.Discount);

      TaxPct = _sqlDb.Taxes_GetBySaleId(SaleId);
      Tax = (Subtotal - Discount) * (TaxPct / 100f);
      Total = Subtotal - Discount + Tax;

      Transactions = _sqlDb.Transactions_GetBySaleId(SaleId).Where(t => t.Type == "Checkout").ToList();
      Change = Transactions.Sum(t => t.Amount) - Total;
    }
  }
}
