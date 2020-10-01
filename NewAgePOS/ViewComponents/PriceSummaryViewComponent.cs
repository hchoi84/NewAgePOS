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
      PriceSummaryViewModel model = GenerateViewModel(saleId);
      model.SaleId = saleId;
      model.IsFromRefund = ViewContext.View.Path.Contains(PathSourceEnum.Refund.ToString());

      return View(model);
    }

    private PriceSummaryViewModel GenerateViewModel(int saleId)
    {
      IEnumerable<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(saleId);
      IEnumerable<SaleLineModel> tradeInSaleLines = saleLines.Where(s => !s.ProductId.HasValue && !s.GiftCardId.HasValue);

      IEnumerable<TransactionModel> transactions = _sqlDb.Transactions_GetBySaleId(saleId);
      TransactionModel giveTransaction = new TransactionModel();
      TransactionModel cashTransaction = new TransactionModel();
      TransactionModel changeTransaction = new TransactionModel();
      List<TransactionModel> giftCardTransactions = new List<TransactionModel>();
      List<TransactionModel> refundTransactions = new List<TransactionModel>();
      foreach (var t in transactions)
      {
        if (t.Type == TypeEnum.Checkout)
        {
          if (t.Method == MethodEnum.Give) 
            giveTransaction = t;
          else if (t.Method == MethodEnum.Cash) 
            cashTransaction = t;
          else if (t.Method == MethodEnum.GiftCard)
            giftCardTransactions.Add(t);
          else if (t.Method == MethodEnum.Change) 
            changeTransaction = t;
        }
        else
        {
          refundTransactions.Add(t);
        }
      }

      IEnumerable<RefundLineModel> refundingLines = _sqlDb.RefundLines_GetBySaleId(saleId)
        .Where(r => !r.TransactionId.HasValue);
      TaxModel tax = _sqlDb.Taxes_GetBySaleId(saleId);

      PriceSummaryViewModel m = new PriceSummaryViewModel();
      m.Quantity = saleLines.Sum(s => s.Qty);
      m.Subtotal = saleLines.Sum(s => s.Subtotal) - tradeInSaleLines.Sum(s => s.Subtotal);

      float giveAmount = giveTransaction == null ? 0 : giveTransaction.Amount;
      m.Discount = saleLines.Sum(s => s.Discount) + giveAmount;
      m.TradeInValue = tradeInSaleLines.Sum(t => t.Price);
      m.TaxPercent = tax.TaxPct;
      m.Paid = giftCardTransactions.Sum(t => t.Amount) + cashTransaction.Amount;
      m.Change = changeTransaction == null ? 0 : changeTransaction.Amount;
      m.RefundedAmount = refundTransactions.Sum(t => t.Amount);

      float refundingAmount = 0;
      if (refundingLines.Any())
      {
        foreach (var refunding in refundingLines)
        {
          SaleLineModel saleLine = saleLines.FirstOrDefault(s => s.Id == refunding.SaleLineId);
          refundingAmount += saleLine.Total / saleLine.Qty * refunding.Qty * (1 + (m.TaxPercent / 100f));
        }
      }
      float refundableAmount = m.Total - m.RefundedAmount;
      m.RefundingAmount = refundableAmount < refundingAmount ? 
        refundableAmount : refundingAmount;

      return m;
    }
  }
}
