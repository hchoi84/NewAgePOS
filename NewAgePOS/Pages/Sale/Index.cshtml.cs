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
      //if (SaleId > 0)
      //  SearchResults = _sqlDb.Sales_GetById(SaleId.ToString());
      //else if (!string.IsNullOrEmpty(LastName))
      //  SearchResults = _sqlDb.Sales_GetByLastName(LastName);
      //else if (!string.IsNullOrEmpty(EmailAddress))
      //  SearchResults = _sqlDb.Sales_GetByEmailAddress(EmailAddress);
      //else if (PhoneNumber > 0)
      //  SearchResults = _sqlDb.Sales_GetByPhoneNumber(PhoneNumber.ToString());
    }

    public IActionResult OnPost()
    {
      if (SaleId > 0) return RedirectToPage(new { SaleId });
      else if (!string.IsNullOrEmpty(LastName)) return RedirectToPage(new { LastName });
      else if (!string.IsNullOrEmpty(EmailAddress)) return RedirectToPage(new { EmailAddress });
      else if (PhoneNumber > 0) return RedirectToPage(new { PhoneNumber });

      return RedirectToPage();
    }

    public IActionResult OnPostCreateNewSale()
    {
      int saleId = _sqlDb.Sales_Insert();
      return RedirectToPage("Cart", new { saleId });
    }
  }
}
