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

    private string[] _userIds = new string[]
    {
      "5422", "6993", "3162", "6679", "2484", "2038", "3125", "6592", "1849", "3620"
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

        SearchBySaleId(saleId);
      }
      else if (SearchMethod == "LastName")
      {
        bool isValid = SearchQuery.Contains(string.Empty);

        if (!isValid)
        {
          TempData["Message"] = "Cannot contain space";
          return Page();
        }

        SearchByLastName();
      }
      else if (SearchMethod == "EmailAddress")
      {
        if (!SearchQuery.Contains("@"))
        {
          TempData["Message"] = "Not a valid email address";
          return Page();
        }

        SearchByEmailAddress();
      }
      else
      {
        bool isValid = long.TryParse(SearchQuery, out long phoneNumber);

        if (!isValid || SearchQuery.Length != 10)
        {
          TempData["Message"] = $"Invalid Phone Number ({ phoneNumber }). 10 numeric characters only";
          return Page();
        }

        SearchByPhoneNumber();
      }

      return Page();
    }

    private void SearchBySaleId(int saleId)
    {
      SaleModel sale = _sqlDb.Sales_GetById(saleId);
      if (sale == null) return;
      CustomerModel customer = _sqlDb.Customers_GetBySaleId(saleId);
      GenerateResults(sale, customer);
    }

    private void SearchByLastName()
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

    private void SearchByEmailAddress()
    {
      CustomerModel customer = _sqlDb.Customers_GetByEmailAddress(SearchQuery);
      if (customer == null) return;
      List<SaleModel> sales = _sqlDb.Sales_GetByCustomerId(customer.Id)
        .OrderByDescending(s => s.Created)
        .ToList();

      foreach (var sale in sales)
        GenerateResults(sale, customer);
    }

    private void SearchByPhoneNumber()
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
        UserId = sale.UserId,
        IsComplete = sale.IsComplete,
        FullName = customer.FullName,
        EmailAddress = customer.EmailAddress,
        PhoneNumber = customer.PhoneNumber,
        MessageCount = _sqlDb.Messages_GetCountBySaleId(sale.Id),
        Created = sale.Created,
      });
    }

    public IActionResult OnPostCreateNewSale(string userId)
    {
      if (string.IsNullOrEmpty(userId))
      {
        TempData["Message"] = "User Id is required to create new sale";
        return Page();
      }

      if (!_userIds.Contains(userId))
      {
        TempData["Message"] = "User Id does not exist";
        return Page();
      }

      int saleId = _sqlDb.Sales_Insert(userId);
      return RedirectToPage("Cart", new { saleId });
    }

    public IActionResult OnPostCancelSale(int saleId)
    {
      SaleModel sale = _sqlDb.Sales_GetById(saleId);
      if (sale.IsComplete)
      {
        TempData["Message"] = $"Sale Id {saleId} can not be cancelled because it is completed";
        return RedirectToPage();
      }

      List<TransactionModel> transactions = _sqlDb.Transactions_GetBySaleId(saleId);
      if (transactions.FirstOrDefault(t => t.Method != MethodEnum.Give) != null)
      {
        TempData["Message"] = $"Sale Id {saleId} can not be cancelled because it has pending transaction(s)";
        return RedirectToPage();
      }

      TransactionModel giveTransaction = transactions.FirstOrDefault(t => t.Method == MethodEnum.Give);
      if (giveTransaction != null)
        _sqlDb.Transactions_DeleteById(giveTransaction.Id);

      List<int> giftCardIds = _sqlDb.SaleLines_GetBySaleId(saleId)
        .Where(s => s.GiftCardId.HasValue)
        .Select(s => s.GiftCardId.Value)
        .ToList();

      _sqlDb.Sales_CancelById(saleId);

      if (giftCardIds.Count > 0)
        giftCardIds.ForEach(id => _sqlDb.GiftCards_Delete(id));

      TempData["Message"] = "Sale has been cancelled";
      return RedirectToPage(new { SearchMethod, SearchQuery });
    }

    public IActionResult OnGetPendingSales()
    {
      Results = new List<SearchViewModel>();

      _sqlDb.Sales_GetPending()
        .ForEach(s =>
        {
          CustomerModel customer = _sqlDb.Customers_GetBySaleId(s.Id);
          GenerateResults(s, customer);
        });

      return Page();
    }
  }
}
