using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;

namespace NewAgePOS.Pages.Sale
{
  public class DetailModel : PageModel
  {
    private readonly ISQLData _sqlDb;

    public DetailModel(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
    }

    public void OnGet()
    {
    }

    public void OnPostCreateMessage(int saleId, string message)
    {
      _sqlDb.Messages_Insert(saleId, message);
    }
  }
}
