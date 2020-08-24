using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using NewAgePOSLibrary.Data;
using NewAgePOSLibrary.Models;

namespace NewAgePOS.Pages.Sale
{
  public class GuestModel : PageModel
  {
    private readonly ISQLData _sqlDb;

    public GuestModel(ISQLData _sqlDb)
    {
      this._sqlDb = _sqlDb;
    }

    [BindProperty(SupportsGet = true)]
    public int SaleId { get; set; }

    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    [Required]
    [Display(Name = "First Name")]
    [MinLength(2, ErrorMessage = "Min {1} Characters")]
    public string FirstName { get; set; }

    [BindProperty]
    [Required]
    [Display(Name = "Last Name")]
    [MinLength(2, ErrorMessage = "Min {1} Characters")]
    public string LastName { get; set; }

    [BindProperty]
    [Required]
    [Display(Name = "Email Address")]
    [DataType(DataType.EmailAddress)]
    public string EmailAddress { get; set; }

    [BindProperty]
    [Required]
    [Display(Name = "Phone Number")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "10 digit numbers only")]
    [RegularExpression(@"\d{10}")]
    public string PhoneNumber { get; set; }

    public IActionResult OnGet()
    {
      CustomerModel customer = _sqlDb.Customers_GetBySaleId(SaleId);
      if (customer.EmailAddress == "guest@email.com") return Page();
      else if (customer != null)
      {
        TextInfo ti = new CultureInfo("en-US", false).TextInfo;

        Id = customer.Id;
        FirstName = ti.ToTitleCase(customer.FirstName);
        LastName = ti.ToTitleCase(customer.LastName);
        EmailAddress = customer.EmailAddress;
        PhoneNumber = customer.PhoneNumber;
      }

      return Page();
    }

    public IActionResult OnPostUseInput()
    {
      if (!ModelState.IsValid) return Page();

      List<CustomerModel> customers = _sqlDb.Customers_GetByEmailOrPhone(EmailAddress, PhoneNumber);
      if (customers != null)
      {
        TempData["Message"] = "Customer with the Email Address or Phone Number already exists. Information from the database have been used";
        _sqlDb.Sales_UpdateCustomerId(SaleId, customers.First().Id);
        return RedirectToPage();
      }

      int customerId = _sqlDb.Customers_Insert(FirstName.Trim().ToLower(),
                              LastName.Trim().ToLower(),
                              EmailAddress.Trim().ToLower(),
                              PhoneNumber);

      _sqlDb.Sales_UpdateCustomerId(SaleId, customerId);

      TempData["Message"] = "Customer has been created";

      return RedirectToPage();
    }

    public IActionResult OnPostEdit()
    {
      if (!ModelState.IsValid) return Page();

      List<CustomerModel> customers = _sqlDb.Customers_GetByEmailOrPhone(EmailAddress, PhoneNumber);
      if (customers.Count() > 1)
      {
        ModelState.AddModelError(string.Empty, "Customer with the Email Address or Phone Number already exists");
        return Page();
      }

      _sqlDb.Customers_Update(Id, FirstName, LastName, EmailAddress, PhoneNumber);
      TempData["Message"] = "Customer has been updated";

      return RedirectToPage();
    }
  }
}
