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

    public PriceSummaryViewComponent(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
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
      List<TransactionModel> paidTransactions = transactions.Where(t => t.Type == TypeEnum.Checkout).ToList();
      List<RefundLineModel> refundLines = _sqlDb.RefundLines_GetBySaleId(saleId);
      TaxModel tax = _sqlDb.Taxes_GetBySaleId(saleId);

      PriceSummaryViewModel model = new PriceSummaryViewModel
      {
        Quantity = saleLines.Sum(s => s.Qty),
        Subtotal = saleLines.Where(s =>
            s.ProductId.HasValue || s.GiftCardId.HasValue)
          .Sum(s => s.LineTotal),
        DiscountAmount = saleLines.Sum(s => s.LineDiscountTotal),
        GiveAmount = paidTransactions.Where(p =>
            p.Type == TypeEnum.Checkout &&
            p.Method == MethodEnum.Give)
          .Sum(p => p.Amount),
        TradeInAmount = saleLines.Where(s => !s.ProductId.HasValue && !s.GiftCardId.HasValue)
          .Sum(s => s.LineTotal),
        TaxPct = tax.TaxPct,
        PaidGiftCard = paidTransactions.Where(p =>
            p.Type == TypeEnum.Checkout &&
            p.Method == MethodEnum.GiftCard)
          .Sum(p => p.Amount),
        PaidCash = paidTransactions.Where(p =>
            p.Type == TypeEnum.Checkout &&
            p.Method == MethodEnum.Cash)
          .Sum(p => p.Amount),
        Change = transactions.FirstOrDefault(t => t.Method == MethodEnum.Change) == null ? 0
          : transactions.FirstOrDefault(t => t.Method == MethodEnum.Change).Amount,
        RefundedAmount = transactions.Where(t => t.Type == TypeEnum.Refund)
          .Sum(t => t.Amount),
        RefundingAmount = GetRefundingAmount(saleId, refundLines, saleLines, transactions)
      };

      return model;
    }

    private float GetRefundingAmount(int saleId, List<RefundLineModel> refundLines, List<SaleLineModel> saleLines, List<TransactionModel> transactions)
    {
      if (ViewContext.View.Path.Contains("Checkout")) return 0;

      List<RefundLineModel> refundingLines = refundLines.Where(rl => rl.TransactionId == 0).ToList();
      if (!refundingLines.Any())
      {
        return 0;
      }

      float refundingAmount = refundingLines.Sum(rl =>
      {
        SaleLineModel saleLine = saleLines.FirstOrDefault(sl => sl.Id == rl.SaleLineId);
        float priceAfterDiscount = saleLine.Price - saleLine.LineDiscountTotal / saleLine.Qty;
        return rl.Qty * priceAfterDiscount;
      });

      float refundableAmount = 0;
      foreach (var t in transactions)
      {
        if (t.Type == TypeEnum.Checkout)
        {
          if (t.Method == MethodEnum.GiftCard || t.Method == MethodEnum.Cash)
            refundableAmount += t.Amount;
          else
            refundableAmount -= t.Amount;
        }
        else
        {
          refundableAmount -= t.Amount;
        }
      }

      if (refundableAmount < refundingAmount)
        refundingAmount = refundableAmount;

      return refundableAmount;
    }
  }
}
