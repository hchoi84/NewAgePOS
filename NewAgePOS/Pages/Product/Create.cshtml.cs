using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;

namespace NewAgePOS.Pages.Product
{
  public class CreateModel : PageModel
  {
    private readonly ISQLData _sqlDb;

    public CreateModel(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
    }

    [BindProperty]
    [Required]
    [RegularExpression(@"\D{3}\d{4}_\d{3}", ErrorMessage = "Must match typical SKU format (ex: AAA0001_001)")]
    public string Sku { get; set; }

    [BindProperty]
    [Required]
    [StringLength(13, MinimumLength = 12, ErrorMessage = "{2} to {1} characters long")]
    [RegularExpression(@"\d{12,13}")]
    public string Upc { get; set; }

    [BindProperty]
    [Required]
    [Range(0f, float.MaxValue, ErrorMessage = "{1} to {2}")]
    public float Cost { get; set; }

    [BindProperty]
    [Required]
    [Range(0f, float.MaxValue, ErrorMessage = "{1} to {2}")]
    public float Price { get; set; }

    [BindProperty]
    [Required]
    [Display(Name = "All Name")]
    [StringLength(150, MinimumLength = 5, ErrorMessage = "{2} to {1} characters long")]
    public string AllName { get; set; }

    [BindProperty]
    public ProductModel ProductFromDb { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
      if (!ModelState.IsValid) return Page();

      ProductFromDb = _sqlDb.Products_Manual_GetByCode(Sku, Upc);

      if (ProductFromDb != null)
      {
        ModelState.AddModelError(string.Empty, "Product already exists in DB");
        return Page();
      }

     _sqlDb.Products_Manual_Insert(Sku, Upc, Cost, Price, AllName);
     TempData["Message"] = "Product added successfully";

      return RedirectToPage();
    }
  }
}
