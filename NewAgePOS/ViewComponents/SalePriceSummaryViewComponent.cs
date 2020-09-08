using Microsoft.AspNetCore.Mvc;
using NewAgePOS.ViewModels.Shared;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;
using System.Collections.Generic;
using System.Linq;

namespace NewAgePOS.ViewComponents
{
  public class SalePriceSummaryViewComponent : ViewComponent
  {
    private readonly ISQLData _sqlDb;

    public SalePriceSummaryViewComponent(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
    }

    public IViewComponentResult Invoke(int saleId)
    {
      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(saleId);
      List<TransactionModel> transactions = _sqlDb.Transactions_GetBySaleId(saleId);
      TaxModel tax = _sqlDb.Taxes_GetBySaleId(saleId);

      PriceSummaryViewModel model = new PriceSummaryViewModel();
      model.Subtotal = saleLines.Sum(s => s.LineTotal);
      model.Discount = saleLines.Sum(s => s.Discount);
      model.Paid = transactions.Where(t => t.Type == "Checkout").Sum(t => t.Amount);
      model.TaxPct = tax.TaxPct;
      
      return View(model);
    }
  }
}
