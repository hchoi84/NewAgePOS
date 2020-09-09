using Microsoft.AspNetCore.Mvc;
using NewAgePOS.Utilities;
using NewAgePOS.ViewModels.Sale;
using System.Collections.Generic;

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
      string path = ViewContext.View.Path;
      // TODO: Determine source and modify View accordingly
      // If discount is being applied, do a cross out of original
      // Cart will have discount
      // Refund will have refunded and refunding quantity
      
      List<ItemListViewModel> items = _share.GenerateItemListViewModel(saleId);
      return View(items);
    }
  }
}
