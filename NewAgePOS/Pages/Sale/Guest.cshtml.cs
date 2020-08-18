using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

    [BindProperty(SupportsGet = true)]
    public CustomerModel Customer { get; set; }

    public void OnGet()
    {
      Customer = _sqlDb.Customers_GetBySaleId(SaleId);
      if (Customer != null)
      {
        TextInfo ti = new CultureInfo("en-US", false).TextInfo;

        Customer.FirstName = ti.ToTitleCase(Customer.FirstName);
        Customer.LastName = ti.ToTitleCase(Customer.LastName);
      }
    }

    public IActionResult OnPostUseInput()
    {
      bool isCorrectFormat = long.TryParse(Customer.PhoneNumber, out long result);

      // TODO: Add error message
      if (!isCorrectFormat) return Page();

      int customerId = _sqlDb.Customers_Insert(Customer.FirstName.Trim().ToLower(),
                              Customer.LastName.Trim().ToLower(),
                              Customer.EmailAddress.Trim().ToLower(),
                              Customer.PhoneNumber);

      _sqlDb.Sales_UpdateCustomerId(SaleId, customerId);

      return RedirectToPage();
    }
  }
}
