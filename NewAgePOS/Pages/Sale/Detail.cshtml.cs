using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;

namespace NewAgePOS.Pages.Sale
{
  public class DetailModel : PageModel
  {
    private readonly ISQLData _sqlDb;

    public DetailModel(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
    }

    [BindProperty(SupportsGet = true)]
    public int SaleId { get; set; }

    public IEnumerable<TransactionModel> Transactions { get; set; }
    public List<GiftCardModel> UsedAsPaymentGCs { get; set; }

    public IActionResult OnGet()
    {
      SaleModel sale = _sqlDb.Sales_GetById(SaleId);
      if (sale == null)
      {
        TempData["Message"] = "Sale Id you provided does not exist";
        return RedirectToPage("/Sale/Search");
      }

      if (!sale.IsComplete)
      {
        TempData["Message"] = "The Sale Id you provided has not been completed yet";
        return RedirectToPage("/Sale/Search");
      }

      Transactions = _sqlDb.Transactions_GetBySaleId(SaleId);
      UsedAsPaymentGCs = new List<GiftCardModel>();

      foreach (var t in Transactions)
      {
        if (t.Method == MethodEnum.GiftCard)
          UsedAsPaymentGCs.Add(_sqlDb.GiftCards_GetById(t.GiftCardId.Value));
      }

      return Page();
    }
  }
}
