using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;
using NewAgePOSLibrary.Databases;
using NewAgePOSLibrary.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewAgePOS.Pages.Sale
{
  public class IndexModel : PageModel
  {
    private readonly ISQLData _sqlDb;

    public IndexModel(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
    }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Sale Id")]
    public int SaleId { get; set; }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Email Address")]
    public string EmailAddress { get; set; }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Phone Number")]
    public long PhoneNumber { get; set; }

    public List<SaleSearchModel> SearchResults { get; set; }

    public void OnGet()
    {
      if (SaleId > 0)
        SearchResults = _sqlDb.GetSalesBySaleId(SaleId.ToString());
      else if (!string.IsNullOrEmpty(LastName))
        SearchResults = _sqlDb.GetSalesByLastName(LastName);
      else if (!string.IsNullOrEmpty(EmailAddress))
        SearchResults = _sqlDb.GetSalesByEmailAddress(EmailAddress);
      else if (PhoneNumber > 0)
        SearchResults = _sqlDb.GetSalesByPhoneNumber(PhoneNumber.ToString());
    }

    public IActionResult OnPost()
    {
      if (SaleId > 0) return RedirectToPage(new { SaleId });
      else if (!string.IsNullOrEmpty(LastName)) return RedirectToPage(new { LastName });
      else if (!string.IsNullOrEmpty(EmailAddress)) return RedirectToPage(new { EmailAddress });
      else if (PhoneNumber > 0) return RedirectToPage(new { PhoneNumber });

      return RedirectToPage();
    }
  }
}
