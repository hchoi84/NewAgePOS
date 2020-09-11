using Microsoft.AspNetCore.Mvc;
using NewAgePOS.Utilities;
using NewAgePOS.ViewModels.Shared;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;
using System.Collections.Generic;
using System.Linq;

namespace NewAgePOS.ViewComponents
{
  public class PriceSummaryViewComponent : ViewComponent
  {
    private readonly ISQLData _sqlDb;
    private readonly IShare _share;

    public PriceSummaryViewComponent(ISQLData sqlDb, IShare share)
    {
      _sqlDb = sqlDb;
      _share = share;
    }

    public IViewComponentResult Invoke(int saleId)
    {
      PriceSummaryViewModel model = GeneratePriceSummaryViewModel(saleId);

      return View(model);
    }

    private PriceSummaryViewModel GeneratePriceSummaryViewModel(int saleId)
    {
      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(saleId);
      List<TransactionModel> transactions = _sqlDb.Transactions_GetBySaleId(saleId);
      List<TransactionModel> paidTransactions = transactions.Where(t => t.Type == "Checkout").ToList();
      List<RefundLineModel> refundLines = _sqlDb.RefundLines_GetBySaleId(saleId);
      TaxModel tax = _sqlDb.Taxes_GetBySaleId(saleId);

      PriceSummaryViewModel model = new PriceSummaryViewModel
      {
        Quantity = saleLines.Sum(s => s.Qty),
        Subtotal = saleLines.Sum(s => s.LineTotal),
        Discount = saleLines.Sum(s => s.LineDiscountTotal),
        TaxPct = tax.TaxPct,
        PaidGiftCard = paidTransactions.Where(p => p.Method == "GiftCard").Sum(p => p.Amount),
        PaidCash = paidTransactions.Where(p => p.Method == "Cash").Sum(p => p.Amount),
        PaidGive = paidTransactions.Where(p => p.Method == "Give").Sum(p => p.Amount),
        RefundedAmount = transactions.Where(t => t.Type == "Refund").Sum(t => t.Amount),
        RefundingAmount = refundLines.Where(rl => rl.TransactionId == 0)
        .Sum(rl => {
          SaleLineModel saleLine = saleLines.FirstOrDefault(sl => sl.Id == rl.SaleLineId);
          return rl.Qty * (saleLine.Price - saleLine.LineDiscountTotal / (float)saleLine.Qty);
        })
      };

      return model;
    }
  }
}
