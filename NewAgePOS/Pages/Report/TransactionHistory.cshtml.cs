using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;

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
        BeginDate = DateTime.Now;
        EndDate = DateTime.Now;
        return Page();
      }
      else Transactions = _sqlDb.Transactions_GetByDateRange(BeginDate, EndDate);

      return Page();
    }
  }
}
