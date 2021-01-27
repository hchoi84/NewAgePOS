using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;
using NewAgePOSModels.Securities;
using NewAgePOSModels.Utilities;

namespace NewAgePOS.Pages.Report
{
  public class TransactionHistoryModel : PageModel
  {
    private readonly ISQLData _sqlDb;

    public TransactionHistoryModel(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
    }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Begin Date")]
    [DataType(DataType.Date)]
    public DateTime BeginDate { get; set; }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "End Date")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    public List<TransactionModel> Transactions { get; private set; }
    public Dictionary<int, int> MessagesCount { get; private set; }
    public Dictionary<int, string> Helper { get; private set; }
    public Dictionary<int, List<string>> Products { get; private set; }
    public Dictionary<int, string> ItemCount { get; private set; }

    public async Task<IActionResult> OnGet()
    {
      if (BeginDate == new DateTime() || EndDate == new DateTime())
      {
        if (Secrets.DBIsLocal)
        {
          BeginDate = DateTime.Now.Date;
          EndDate = DateTime.Now.Date;
        }
        else
        {
          BeginDate = DateTime.UtcNow.UTCtoPST().Date;
          EndDate = DateTime.UtcNow.UTCtoPST().Date;
        }
        return Page();
      }

      InitializeTransactions();

      IEnumerable<int> saleIds = Transactions.Select(t => t.SaleId).Distinct();
      List<Task> tasks = new List<Task>
      {
        Task.Run(() => InitializeMessageCount(saleIds)),
        Task.Run(() => InitializeHelper(saleIds)),
        Task.Run(() => InitializeProducts(saleIds))
      };

      await Task.WhenAll(tasks);

      return Page();
    }

    private void InitializeTransactions()
    {
      Transactions = _sqlDb.Transactions_GetByDateRange(BeginDate.PSTtoUTC(), EndDate.AddDays(1).PSTtoUTC())
          .Where(t => t.Method != MethodEnum.Give && Math.Round(t.Amount, 2) != 0f)
          .OrderBy(t => t.SaleId)
          .ThenBy(t => t.Type)
          .ThenBy(t => t.Method)
          .ToList();

      foreach (var transaction in Transactions)
      {
        if (transaction.Method == MethodEnum.Change)
        {
          var cashTransaction = Transactions.FirstOrDefault(t =>
            t.SaleId == transaction.SaleId &&
            t.Type == TypeEnum.Checkout &&
            t.Method == MethodEnum.Cash);

          if (cashTransaction != null)
          {
            cashTransaction.Amount -= transaction.Amount;
          }
        }
      }

      Transactions.RemoveAll(t => t.Method == MethodEnum.Change);
    }

    private void InitializeMessageCount(IEnumerable<int> saleIds)
    {
      MessagesCount = new Dictionary<int, int>();

      foreach (var saleId in saleIds)
      {
        MessagesCount.Add(saleId, _sqlDb.Messages_GetCountBySaleId(saleId));
      }
    }

    private void InitializeHelper(IEnumerable<int> saleIds)
    {
      Helper = new Dictionary<int, string>();

      foreach (var saleId in saleIds)
      {
        string helperId = _sqlDb.Sales_GetHelperId(saleId);

        Helper.Add(saleId, helperId);
      }
    }

    private void InitializeProducts(IEnumerable<int> saleIds)
    {
      Products = new Dictionary<int, List<string>>();
      ItemCount = new Dictionary<int, string>();

      foreach (var saleId in saleIds)
      {
        IEnumerable<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(saleId);

        InitializeItemCountByCategory(saleId, saleLines);

        List<string> allNames = new List<string>();

        foreach (var saleLine in saleLines.Where(x => x.ProductId.HasValue))
        {
          ProductModel product = _sqlDb.Products_GetById(saleLine.ProductId.Value);
          float afterDiscount = product.Price * (1 - (saleLine.DiscPct / 100f));
          allNames.Add($"{product.AllName} ({afterDiscount:C})");
        }

        Products.Add(saleId, allNames);
      }
    }

    private void InitializeItemCountByCategory(int saleId, IEnumerable<SaleLineModel> saleLines)
    {
      int productCount = saleLines.Where(x => x.ProductId.HasValue).Count();
      int giftCardCount = saleLines.Where(x => x.GiftCardId.HasValue).Count();
      int tradeInCount = saleLines.Where(x => !x.ProductId.HasValue && !x.GiftCardId.HasValue).Count();
      ItemCount.Add(saleId, $"{ productCount } / { giftCardCount } / { tradeInCount }");
    }
  }
}
