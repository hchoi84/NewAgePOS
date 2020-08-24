using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;
using NewAgePOSLibrary.Models;

namespace NewAgePOS.Pages.Product
{
  public class SearchModel : PageModel
  {
    private readonly ISQLData _sqlDb;

    public SearchModel(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
    }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "SKU or UPC")]
    public string Code { get; set; }

    public List<ProductDbModel> Products { get; set; }

    public IActionResult OnGet()
    {
      if (string.IsNullOrEmpty(Code)) return Page();

      if (Code.Length == 7)
        Products = _sqlDb.Products_GetByParentSku(Code);
      else if (Code.Contains("_"))
      {
        Products = new List<ProductDbModel>();
        Products.Add(_sqlDb.Products_GetByCode(Code, "") ?? null);
      }
      else if (Code.Length >= 12)
      {
        Products = new List<ProductDbModel>();
        Products.Add(_sqlDb.Products_GetByCode("", Code) ?? null);
      }
      else
        TempData["Message"] = "No results found";

      return Page();
    }

    public IActionResult OnPost()
    {
      return RedirectToPage(new { Code });
    }
  }
}
