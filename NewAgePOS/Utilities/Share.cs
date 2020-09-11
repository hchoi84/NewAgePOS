using NewAgePOS.ViewModels.Sale;
using NewAgePOS.ViewModels.Shared;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewAgePOS.Utilities
{
  public class Share : IShare
  {
    private readonly ISQLData _sqlDb;

    public Share(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
    }

    public PriceSummaryViewModel GeneratePriceSummaryViewModel(int saleId)
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

    public List<ItemListViewModel> GenerateItemListViewModel(int saleId)
    {
      List<ItemListViewModel> items = new List<ItemListViewModel>();
      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(saleId);
      List<ProductModel> products = _sqlDb.Products_GetBySaleId(saleId);
      List<GiftCardModel> giftCards = _sqlDb.GiftCards_GetBySaleId(saleId);

      foreach (SaleLineModel saleLine in saleLines)
      {
        ProductModel product = new ProductModel();
        GiftCardModel giftCard = new GiftCardModel();

        bool isProduct = saleLine.ProductId.HasValue;
        ItemListViewModel item = new ItemListViewModel();

        if (isProduct)
        {
          product = products.FirstOrDefault(p => p.Id == saleLine.ProductId.Value);

          item.SaleLineId = saleLine.Id;
          item.IsProduct = isProduct;
          item.Sku = product.Sku;
          item.Upc = product.Upc;
          item.AllName = product.AllName;
          item.Cost = saleLine.Cost;
          item.Price = saleLine.Price;
          item.Qty = saleLine.Qty;
          item.DiscPct = saleLine.DiscPct;
        }
        else
        {
          giftCard = giftCards.FirstOrDefault(g => g.Id == saleLine.GiftCardId.Value);

          item.SaleLineId = saleLine.Id;
          item.IsProduct = isProduct;
          item.Sku = giftCard.Code;
          item.Price = giftCard.Amount;
          item.Qty = 1;
        }

        items.Add(item);
      }

      return items;
    }
  }
}
