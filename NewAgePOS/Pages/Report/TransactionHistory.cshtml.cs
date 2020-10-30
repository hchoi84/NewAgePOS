using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

    public List<TransactionModel> Transactions { get; set; }
    public Dictionary<int, int> MessagesCount { get; set; }

    public IActionResult OnGet()
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

      Initialize();

      return Page();
    }

    private void Initialize()
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

      MessagesCount = new Dictionary<int, int>();
      IEnumerable<int> saleIds = Transactions.Select(t => t.SaleId).Distinct();
      foreach (var saleId in saleIds)
      {
        MessagesCount.Add(saleId, _sqlDb.Messages_GetCountBySaleId(saleId));
      }
    }
  }
}
