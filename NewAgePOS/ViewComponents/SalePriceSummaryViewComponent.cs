using Microsoft.AspNetCore.Mvc;
using NewAgePOSModels.Models;
using System.Collections.Generic;
using System.Linq;

namespace NewAgePOS.ViewComponents
{
  public class SalePriceSummaryViewComponent : ViewComponent
  {
    public IViewComponentResult Invoke(List<SaleLineModel> saleLines, float taxPct, List<TransactionModel> transactions = null)
    {
      SalePriceSummaryModel model = new SalePriceSummaryModel();
      model.Subtotal = saleLines.Sum(sl => sl.LineTotal);
      model.Discount = saleLines.Sum(sl => sl.Discount);
      model.Paid = transactions == null ? 0 : transactions.Sum(t => t.Amount);
      model.TaxPct = taxPct;
      model.Tax = (model.Subtotal - model.Discount) * (taxPct / 100f);
      model.Total = model.Subtotal - model.Discount - model.Paid + model.Tax;
      
      return View(model);
    }
  }
}
