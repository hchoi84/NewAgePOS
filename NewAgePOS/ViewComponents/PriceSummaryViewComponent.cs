using Microsoft.AspNetCore.Mvc;
using NewAgePOS.Utilities;
using NewAgePOS.ViewModels.Shared;

namespace NewAgePOS.ViewComponents
{
  public class PriceSummaryViewComponent : ViewComponent
  {
    private readonly IShare _share;

    public PriceSummaryViewComponent(IShare share)
    {
      _share = share;
    }

    public IViewComponentResult Invoke(int saleId)
    {
      PriceSummaryViewModel model = _share.GeneratePriceSummaryViewModel(saleId);

      return View(model);
    }
  }
}
