using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;

namespace NewAgePOS.Pages.Report
{
  public class GiftCardsModel : PageModel
  {
    private readonly ISQLData _sqlDb;

    public GiftCardsModel(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
    }

    public string Balance { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnGetCheckBalance(string code)
    {
      var gc = _sqlDb.GiftCards_GetByCode(code);
      if (gc == null)
      {
        TempData["Message"] = $"{ code }: Does not exist";
        return Page();
      }

      Balance = gc.Amount.ToString("C2");

      return Page();
    }
  }
}
