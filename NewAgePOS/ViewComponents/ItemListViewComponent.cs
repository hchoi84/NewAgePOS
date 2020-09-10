using Microsoft.AspNetCore.Mvc;
using NewAgePOS.Utilities;
using NewAgePOS.ViewModels.ViewComponent;
using NewAgePOSLibrary.Data;

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
      // TODO: Refund will have refunded and refunding quantity
      // TODO: Implement towards Refund
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

      items.Items = _share.GenerateItemListViewModel(saleId);
      return View(items);
    }
  }
}
