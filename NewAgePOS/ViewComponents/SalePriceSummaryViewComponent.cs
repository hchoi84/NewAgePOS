using Microsoft.AspNetCore.Mvc;
using NewAgePOS.Utilities;
using NewAgePOS.ViewModels.Shared;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;
using System.Collections.Generic;
using System.Linq;

namespace NewAgePOS.ViewComponents
{
  public class SalePriceSummaryViewComponent : ViewComponent
  {
    private readonly IShare _share;

    public SalePriceSummaryViewComponent(IShare share)
    {
      _share = share;
    }

    public IViewComponentResult Invoke(int saleId)
    {
      PriceSummaryViewModel model = _share.GeneratePriceSummaryViewModel(saleId);

      return View(model);
    }

    public IViewComponentResult Invoke(PriceSummaryViewModel model)
    {
      return View(model);
    }
  }
}
