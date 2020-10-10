using Microsoft.AspNetCore.Mvc;
using NewAgePOS.Utilities;
using NewAgePOS.ViewModels.Sale;
using NewAgePOS.ViewModels.ViewComponent;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;
using System.Collections.Generic;
using System.Linq;

namespace NewAgePOS.ViewComponents
{
  public class ItemListViewComponent : ViewComponent
  {
    private readonly ISQLData _sqlDb;

    public ItemListViewComponent(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
    }

    public IViewComponentResult Invoke(int saleId)
    {
      ItemListVCVM items = new ItemListVCVM();
      string path = ViewContext.View.Path;

      if (path.Contains(PathSourceEnum.Cart.ToString()))
      {
        items.PathSource = PathSourceEnum.Cart;
      }
      else if (path.Contains(PathSourceEnum.Refund.ToString()))
      {
        items.PathSource = PathSourceEnum.Refund;
        items.Refunds = _sqlDb.RefundLines_GetBySaleId(saleId);
      }

      items.Items = GenerateItemListViewModel(saleId)
        .OrderByDescending(i => i.IsProduct)
        .ThenByDescending(i => i.IsGiftCard)
        .ToList();
      return View(items);
    }

    private List<ItemListViewModel> GenerateItemListViewModel(int saleId)
    {
      List<ItemListViewModel> items = new List<ItemListViewModel>();
      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(saleId);
      List<ProductModel> products = _sqlDb.Products_GetBySaleId(saleId);
      List<GiftCardModel> giftCards = _sqlDb.GiftCards_GetBySaleId(saleId);

      foreach (SaleLineModel saleLine in saleLines)
      {
        ItemListViewModel item = new ItemListViewModel();

        if (saleLine.ProductId.HasValue)
        {
          ProductModel product = products.FirstOrDefault(p => p.Id == saleLine.ProductId.Value);

          item.SaleLineId = saleLine.Id;
          item.IsProduct = true;
          item.Sku = product.Sku;
          item.Upc = product.Upc;
          item.AllName = product.AllName;
          item.Cost = saleLine.Cost;
          item.Price = saleLine.Price;
          item.Qty = saleLine.Qty;
          item.DiscPct = saleLine.DiscPct;
        }
        else if (saleLine.GiftCardId.HasValue)
        {
          int gcId = saleLine.GiftCardId.Value;
          GiftCardModel giftCard = giftCards.FirstOrDefault(g => g.Id == gcId);

          item.SaleLineId = saleLine.Id;
          item.IsGiftCard = true;
          item.Sku = giftCard.Code;
          item.Price = saleLine.Price;
          item.Qty = 1;
        }
        else
        {
          item.SaleLineId = saleLine.Id;
          item.Price = saleLine.Price;
          item.Qty = saleLine.Qty;
        }

        items.Add(item);
      }

      return items;
    }
  }
}
