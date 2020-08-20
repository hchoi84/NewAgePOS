using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Globalization;
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

      bool isPhoneNumber = long.TryParse(PhoneNumber, out long ph);
      if (!isPhoneNumber) 
      {
        ModelState.AddModelError(string.Empty, "Digits only");
        return Page();
          }

      int customerId = _sqlDb.Customers_Insert(FirstName.Trim().ToLower(),
                              LastName.Trim().ToLower(),
                              EmailAddress.Trim().ToLower(),
                              PhoneNumber);

      _sqlDb.Sales_UpdateCustomerId(SaleId, customerId);

      TempData["Message"] = "Customer has been created";

      return RedirectToPage();
    }

    // TODO: Implement updating customer information
    public IActionResult OnPostEdit()
    {
      if (!ModelState.IsValid) return Page();

      if (_sqlDb.Customers_GetByEmailOrPhone(EmailAddress, PhoneNumber) > 1)
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
