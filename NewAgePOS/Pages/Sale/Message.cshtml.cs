using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;

namespace NewAgePOS.Pages.Sale
{
  public class MessageModel : PageModel
  {
    private readonly ISQLData _sqlDb;

    public MessageModel(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
    }

    [BindProperty(SupportsGet = true)]
    public int SaleId { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPostCreateMessage(int saleId, string message, string returnUrl)
    {
      if (string.IsNullOrEmpty(message))
      {
        TempData["Message"] = "Message can not be blank";
        if (Url.IsLocalUrl(returnUrl))
          return Redirect(returnUrl);
        else
          return RedirectToPage();
      }

      if (message.Length > 100)
      {
        TempData["Message"] = "Message can not be greater than 100 characters";
        if (Url.IsLocalUrl(returnUrl))
          return Redirect(returnUrl);
        else
          return RedirectToPage();
      }

      _sqlDb.Messages_Insert(saleId, message);
      if (Url.IsLocalUrl(returnUrl))
        return Redirect(returnUrl);
      else
        return RedirectToPage();
    }

    public IActionResult OnPostDeleteMessage(int id)
    {
      _sqlDb.Messages_Delete(id);
      return RedirectToPage();
    }
  }
}
