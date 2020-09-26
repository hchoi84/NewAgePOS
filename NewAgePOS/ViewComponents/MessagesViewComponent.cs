using Microsoft.AspNetCore.Mvc;
using NewAgePOS.ViewModels.ViewComponent;
using NewAgePOSLibrary.Data;
using System.Linq;

namespace NewAgePOS.ViewComponents
{
  public class MessagesViewComponent : ViewComponent
  {
    private readonly ISQLData _sqlDb;

    public MessagesViewComponent(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
    }

    public IViewComponentResult Invoke(int saleId)
    {
      MessagesVCVM model = new MessagesVCVM();
      model.ReturnUrl = HttpContext.Request.Path;
      model.SaleId = saleId;
      model.Messages = _sqlDb.Messages_GetBySaleId(saleId).OrderByDescending(m => m.Created).ToList();

      return View(model);
    }
  }
}
