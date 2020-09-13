using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NewAgePOS.ViewModels.Sale;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;
using System.Collections.Generic;
using System.Linq;

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
    public string SearchMethod { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SearchQuery { get; set; }

    [BindProperty]
    public List<SearchViewModel> Results { get; set; }

    public List<SelectListItem> SearchMethods { get; } = new List<SelectListItem>
    {
      new SelectListItem { Text = "Sale Id", Value = "SaleId" },
      new SelectListItem { Text = "Last Name", Value = "LastName" },
      new SelectListItem { Text = "Email Address", Value = "EmailAddress" },
      new SelectListItem { Text = "Phone Number", Value = "PhoneNumber" },
    };

    public IActionResult OnGet()
    {
      if (string.IsNullOrEmpty(SearchQuery)) return Page();

      SearchQuery = SearchQuery.Trim();
      Results = new List<SearchViewModel>();

      if (SearchMethod == "SaleId")
      {
        bool isValid = int.TryParse(SearchQuery, out int saleId);

        if (!isValid || saleId <= 0)
        {
          TempData["Message"] = "Invalid SaleId";
          return Page();
        }

        ProcessSaleId(saleId);
      }
      else if (SearchMethod == "LastName")
      {
        bool isValid = SearchQuery.Contains(string.Empty);

        if (!isValid)
        {
          TempData["Message"] = "Cannot contain space";
          return Page();
        }

        ProcessLastName();
      }
      else if (SearchMethod == "EmailAddress")
      {
        if (!SearchQuery.Contains("@"))
        {
          TempData["Message"] = "Not a valid email address";
          return Page();
        }

        ProcessEmailAddress();
      }
      else
      {
        bool isValid = long.TryParse(SearchQuery, out long phoneNumber);

        if (!isValid || SearchQuery.Length != 10)
        {
          TempData["Message"] = "Invalid Phone Number. 10 numeric characters only";
          return Page();
        }

        ProcessPhoneNumber();
      }

      return Page();
    }

    private void ProcessSaleId(int saleId)
    {
      SaleModel sale = _sqlDb.Sales_GetById(saleId);
      if (sale == null) return;
      CustomerModel customer = _sqlDb.Customers_GetBySaleId(saleId);
      GenerateResults(sale, customer);
    }

    private void ProcessLastName()
    {
      List<CustomerModel> customers = _sqlDb.Customers_GetByLastName(SearchQuery);
      if (customers == null) return;
      List<SaleModel> sales = customers.Select(c => _sqlDb.Sales_GetByCustomerId(c.Id))
        .SelectMany(s => s)
        .OrderBy(s => s.CustomerId)
        .ThenByDescending(s => s.Created)
        .ToList();

      CustomerModel customer = new CustomerModel();
      for (int i = 0; i < sales.Count; i++)
      {
        if (i == 0 || sales[i].CustomerId != sales[i - 1].CustomerId)
          customer = customers.FirstOrDefault(c => c.Id == sales[i].CustomerId);

        GenerateResults(sales[i], customer);
      }
    }

    private void ProcessEmailAddress()
    {
      CustomerModel customer = _sqlDb.Customers_GetByEmailAddress(SearchQuery);
      if (customer == null) return;
      List<SaleModel> sales = _sqlDb.Sales_GetByCustomerId(customer.Id)
        .OrderByDescending(s => s.Created)
        .ToList();

      foreach (var sale in sales)
        GenerateResults(sale, customer);
    }

    private void ProcessPhoneNumber()
    {
      CustomerModel customer = _sqlDb.Customers_GetByPhoneNumber(SearchQuery);
      if (customer == null) return;
      List<SaleModel> sales = _sqlDb.Sales_GetByCustomerId(customer.Id)
        .OrderByDescending(s => s.Created)
        .ToList();

      foreach (var sale in sales)
        GenerateResults(sale, customer);
    }

    private void GenerateResults(SaleModel sale, CustomerModel customer)
    {
      Results.Add(new SearchViewModel
      {
        SaleId = sale.Id,
        Created = sale.Created,
        IsComplete = sale.IsComplete,
        EmailAddress = customer.EmailAddress,
        FullName = customer.FullName,
        PhoneNumber = customer.PhoneNumber
      });
    }

    public IActionResult OnPostCreateNewSale()
    {
      int saleId = _sqlDb.Sales_Insert();
      return RedirectToPage("Cart", new { saleId });
    }

    public IActionResult OnPostCancelSale(int saleId)
    {
      List<int> giftCardIds = _sqlDb.SaleLines_GetBySaleId(saleId)
        .Where(s => s.GiftCardId.HasValue)
        .Select(s => s.GiftCardId.Value)
        .ToList();

      _sqlDb.Sales_CancelById(saleId);

      if (giftCardIds.Count > 0)
        giftCardIds.ForEach(g => _sqlDb.GiftCards_Delete(g));

      TempData["Message"] = "Sale has been cancelled";
      return RedirectToPage(new { SearchMethod, SearchQuery });
    }

    public IActionResult OnPostReceipt(int saleId)
    {
      return RedirectToPage("Receipt", new { id = saleId, idType = "Sale" });
    }
  }
}
