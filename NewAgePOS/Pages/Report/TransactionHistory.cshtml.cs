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

      Console.WriteLine($"PST: { BeginDate } - UTC: { BeginDate.PSTtoUTC() }");
      Transactions = _sqlDb.Transactions_GetByDateRange(BeginDate.PSTtoUTC(), EndDate.AddDays(1).PSTtoUTC())
          .OrderBy(t => t.SaleId)
          .ThenBy(t => t.Method)
          .ToList();

      return Page();
    }
  }
}
