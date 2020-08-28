using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

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
    [Range(0, int.MaxValue)]
    public int SaleId { get; set; }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Last Name")]
    [MinLength(2, ErrorMessage = "Min {1} characters")]
    public string LastName { get; set; }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Email Address")]
    [DataType(DataType.EmailAddress)]
    public string EmailAddress { get; set; }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Phone Number")]
    [StringLength(10, MinimumLength = 10)]
    public string PhoneNumber { get; set; }

    [BindProperty(SupportsGet = true)]
    public List<SaleSearchResultModel> Results { get; set; }

    public IActionResult OnGet()
    {
      Results = _sqlDb.SearchSales(SaleId, LastName, EmailAddress, PhoneNumber);
      
      if (Results != null)
      {
        TextInfo ti = new CultureInfo("en-US", false).TextInfo;

        Results.ForEach(r => r.FullName = ti.ToTitleCase(r.FullName));
      }

      return Page();
    }

    public IActionResult OnPostCreateNewSale()
    {
      int saleId = _sqlDb.Sales_Insert();
      return RedirectToPage("Cart", new { saleId });
    }

    public IActionResult OnPostCancelSale(int saleId)
    {
      _sqlDb.Sales_CancelById(saleId);
      TempData["Message"] = "Sale has been cancelled";
      return RedirectToPage();
    }
  }
}
