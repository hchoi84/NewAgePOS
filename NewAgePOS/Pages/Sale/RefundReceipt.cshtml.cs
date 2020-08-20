using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;
using NewAgePOSLibrary.Models;

namespace NewAgePOS.Pages.Sale
{
  public class RefundReceiptModel : PageModel
  {
    private readonly ISQLData _sqlDb;

    public RefundReceiptModel(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
    }

    [BindProperty(SupportsGet = true)]
    public int TransactionId { get; set; }

    [BindProperty(SupportsGet = true)]
    public List<RefundDataModel> Refunds { get; set; }

    public void OnGet()
    {
      Refunds = _sqlDb.GetRefundReceiptData(TransactionId);
    }
  }
}
