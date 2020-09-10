using Microsoft.AspNetCore.Mvc;
using NewAgePOS.Utilities;
using NewAgePOS.ViewModels.ViewComponent;

namespace NewAgePOS.ViewComponents
{
  public class ItemListViewComponent : ViewComponent
  {
    private readonly IShare _share;

    public ItemListViewComponent(IShare share)
    {
      _share = share;
    }

    public IViewComponentResult Invoke(int saleId)
    {
      // TODO: Refund will have refunded and refunding quantity
      // TODO: Implement towards Refund
      ItemListVCVM items = new ItemListVCVM();
      string path = ViewContext.View.Path;

      if (path.Contains(PathSourceEnum.Cart.ToString()))
        items.PathSource = PathSourceEnum.Cart;
      
      items.Items = _share.GenerateItemListViewModel(saleId);
      return View(items);
    }
  }
}
