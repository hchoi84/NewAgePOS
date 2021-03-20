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
      string path = ViewContext.View.Path;
      PriceSummaryViewModel model = new PriceSummaryViewModel();

      model.SaleId = saleId;
      if (path.Contains(PathSourceEnum.Refund.ToString()))
        model.PathSource = PathSourceEnum.Refund;
      else if (path.Contains(PathSourceEnum.Checkout.ToString()))
        model.PathSource = PathSourceEnum.Checkout;

      GenerateViewModel(model);
      
      return View(model);
    }

    private void GenerateViewModel(PriceSummaryViewModel model)
    {
      int saleId = model.SaleId;
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

      model.Quantity = saleLines.Sum(s => s.Qty);
      model.Subtotal = saleLines.Sum(s => s.Subtotal) - tradeInSaleLines.Sum(s => s.Subtotal);

      model.Discount = saleLines.Sum(s => s.Discount) + giveTransaction.Amount;
      model.TradeInValue = tradeInSaleLines.Sum(t => t.Price * t.Qty);
      model.TaxPercent = tax.TaxPct;
      model.Paid = giftCardTransactions.Sum(t => t.Amount) + cashTransaction.Amount;
      model.Change = changeTransaction.SaleId == 0 ? 
        model.Paid - model.Total : changeTransaction.Amount;
      model.RefundedAmount = refundTransactions.Sum(t => t.Amount);

      float refundingAmount = 0;
      if (refundingLines.Any())
      {
        foreach (var refunding in refundingLines)
        {
          SaleLineModel saleLine = saleLines.FirstOrDefault(s => s.Id == refunding.SaleLineId);
          refundingAmount += saleLine.Total / saleLine.Qty * refunding.Qty * (1 + (model.TaxPercent / 100f));
        }
      }
      float refundableAmount = model.Total - model.RefundedAmount;
      model.RefundingAmount = refundableAmount < refundingAmount ? 
        refundableAmount : refundingAmount;

      if (model.PathSource == PathSourceEnum.Checkout && giveTransaction.Id == 0)
        CalculateGiveAmount(model);
    }

    private void CalculateGiveAmount(PriceSummaryViewModel model)
    {
      float factor = 0.05f;
      float roundedDueBalance = (int)(model.DueBalance / factor) * factor;
      float giveAmount = model.DueBalance - roundedDueBalance;
      
      if (giveAmount != 0)
      {
        _sqlDb.Transactions_Insert(model.SaleId, null, giveAmount, MethodEnum.Give, TypeEnum.Checkout);
        model.Discount += giveAmount;
      }
    }
  }
}
