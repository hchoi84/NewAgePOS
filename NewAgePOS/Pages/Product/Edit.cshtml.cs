using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;
using NewAgePOSLibrary.Models;

namespace NewAgePOS.Pages.Product
{
  public class EditModel : PageModel
  {
    private readonly ISQLData _sqlDb;

    public EditModel(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [Required]
    [RegularExpression(@"\D{3}\d{4}_\d{3}", ErrorMessage = "Must match typical SKU format (ex: AAA0001_001)")]
    public string Sku { get; set; }

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

    public IActionResult OnGet()
    {
      ProductDbModel product = _sqlDb.Products_GetById(Id);
      if (product == null) return Page();
      if (product.Source == "API")
      {
        TempData["Message"] = "That product is from API and cannot be edited. Please update product information in ChannelAdvisor";
        return RedirectToPage("Search");
      }

      Sku = product.Sku;
      Upc = product.Upc;
      Cost = product.Cost;
      Price = product.Price;
      AllName = product.AllName;

      return Page();
    }

    public IActionResult OnPost()
    {
      if (!ModelState.IsValid) return Page();

      _sqlDb.Products_Update(Id, Cost, Price, AllName);

      TempData["Message"] = "Product added successfully";

      return RedirectToPage("Search");
    }
  }
}
