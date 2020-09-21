using Microsoft.AspNetCore.Mvc;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;
using System.Collections.Generic;
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
      List<MessageModel> messages = _sqlDb.Messages_GetBySaleId(saleId).OrderBy(m => m.Created).ToList();

      return View(messages);
    }
  }
}
