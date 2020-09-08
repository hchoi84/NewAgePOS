using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOS.ViewModels.Sale;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;

namespace NewAgePOS.Pages.Sale
{
  public class GuestModel : PageModel
  {
    private readonly ISQLData _sqlDb;

    public GuestModel(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
    }

    [BindProperty(SupportsGet = true)]
    public int SaleId { get; set; }

    [BindProperty]
    public GuestViewModel Guest { get; set; }

    public IActionResult OnGet()
    {
      CustomerModel customer = _sqlDb.Customers_GetBySaleId(SaleId);
      if (customer.EmailAddress == "guest@email.com") return Page();
      else if (customer != null)
      {
        Guest.Id = customer.Id;
        Guest.FirstName = customer.FirstName;
        Guest.LastName = customer.LastName;
        Guest.EmailAddress = customer.EmailAddress;
        Guest.PhoneNumber = customer.PhoneNumber;
      }

      return Page();
    }

    public IActionResult OnPostUseInput()
    {
      if (!ModelState.IsValid) return Page();

      CustomerModel customer = new CustomerModel();
      customer = _sqlDb.Customers_GetByEmailAddress(Guest.EmailAddress) 
        ?? _sqlDb.Customers_GetByPhoneNumber(Guest.PhoneNumber);

      if (customer != null)
      {
        TempData["Message"] = "Customer found and assigned to this sale";
        _sqlDb.Sales_UpdateCustomerId(SaleId, customer.Id);
        return RedirectToPage();
      }

      int customerId = _sqlDb.Customers_Insert(Guest.FirstName.Trim().ToLower(),
                              Guest.LastName.Trim().ToLower(),
                              Guest.EmailAddress.Trim().ToLower(),
                              Guest.PhoneNumber);

      _sqlDb.Sales_UpdateCustomerId(SaleId, customerId);

      TempData["Message"] = "No existing customer found. New customer created and assigned to this sale";

      return RedirectToPage();
    }

    public IActionResult OnPostEdit()
    {
      if (!ModelState.IsValid) return Page();

      CustomerModel customer = _sqlDb.Customers_GetById(Guest.Id);
      bool isInDb = false;

      if (Guest.EmailAddress != customer.EmailAddress)
        isInDb = _sqlDb.Customers_GetByEmailAddress(Guest.EmailAddress) != null;

      if (Guest.PhoneNumber != customer.PhoneNumber)
        isInDb = _sqlDb.Customers_GetByPhoneNumber(Guest.PhoneNumber) != null;

      if (isInDb)
      {
        ModelState.AddModelError(string.Empty, "Customer with the new Email Address or Phone Number already exists");
        return Page();
      }

      _sqlDb.Customers_Update(Guest.Id, Guest.FirstName, Guest.LastName, Guest.EmailAddress, Guest.PhoneNumber);
      TempData["Message"] = "Customer has been updated";

      return RedirectToPage();
    }
  }
}
