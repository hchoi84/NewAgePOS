using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace NewAgePOS.Pages.Sale
{
  public class SearchModel : PageModel
  {
    private readonly ISQLData _sqlDb;

    public SearchModel(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
    }

    [BindProperty(SupportsGet = true)]
    public int SaleId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SearchMethod { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SearchQuery { get; set; }

    [BindProperty]
    public List<SaleSearchResultModel> Results { get; set; }

    public List<SelectListItem> SearchMethods { get; } = new List<SelectListItem>
    {
      new SelectListItem { Text = "Sale Id", Value = "SaleId" },
      new SelectListItem { Text = "Last Name", Value = "LastName" },
      new SelectListItem { Text = "Email Address", Value = "EmailAddress" },
      new SelectListItem { Text = "Phone Number", Value = "PhoneNumber" },
    };

    public IActionResult OnGet()
    {
      if (string.IsNullOrEmpty(SearchMethod)) return Page();

      if (SearchMethod == "SaleId")
      {
        bool isValid = Int32.TryParse(SearchQuery, out int saleId);

        if (!isValid && saleId > 0)
        {
          TempData["Message"] = "Invalid SaleId";
          return Page();
        }

        Results = _sqlDb.SearchSales(saleId, "", "", "");
      }
      else if (SearchMethod == "LastName")
      {
        Results = _sqlDb.SearchSales(0, SearchQuery, "", "");
      }
      else if (SearchMethod == "EmailAddress")
      {
        if (!SearchQuery.Contains("@"))
        {
          TempData["Message"] = "Not a valid email address";
          return Page();
        }

        Results = _sqlDb.SearchSales(0, "", SearchQuery, "");
      }
      else
      {
        bool isValid = Int64.TryParse(SearchQuery, out long phoneNumber);

        if (isValid && SearchQuery.Length == 10) Results = _sqlDb.SearchSales(0, "", "", SearchQuery);
      }
      
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

    public IActionResult OnPostCancelSale()
    {
      _sqlDb.Sales_CancelById(SaleId);
      TempData["Message"] = "Sale has been cancelled";
      return RedirectToPage(new { SearchMethod, SearchQuery });
    }
  }
}
