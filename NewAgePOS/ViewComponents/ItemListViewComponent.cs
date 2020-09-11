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
    private readonly IShare _share;
    private readonly ISQLData _sqlDb;

    public ItemListViewComponent(IShare share, ISQLData sqlDb)
    {
      _share = share;
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

      items.Items = GenerateItemListViewModel(saleId);
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
