using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;

namespace NewAgePOS.Pages.Sale
{
  public class ReceiptModel : PageModel
  {
    private readonly ISQLData _sqlDb;

    public ReceiptModel(ISQLData _sqlDb)
    {
      this._sqlDb = _sqlDb;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public string IdType { get; set; }

    public string Created { get; set; }
    public string FullName { get; set; }
    public int RefundItems { get; set; }
    public string RefundTotal { get; set; }
    public string RefundMethod { get; set; }

    public IActionResult OnGet()
    {
      CustomerModel customer = new CustomerModel();
      IdType = IdType.ToLower();

      if (IdType == "sale")
      {
        SaleModel sale = _sqlDb.Sales_GetById(Id);
        if (sale == null)
        {
          TempData["Message"] = $"Sale id {Id} was not found";
          return RedirectToPage("Search");
        }

        bool isComplete = sale.IsComplete;
        if (!isComplete)
        {
          TempData["Message"] = $"Sale id {Id} is not complete yet";
          return RedirectToPage("Search");
        }

        customer = _sqlDb.Customers_GetBySaleId(Id);
        FullName = customer.FullName.Contains("Guest") ? "" : customer.FullName;

        Created = _sqlDb.Sales_GetById(Id).Created.ToString("MM/dd/yyyy");
        return Page();
      }
      else if(IdType == "refund")
      {
        TransactionModel transaction = _sqlDb.Transactions_GetById(Id);
        if (transaction == null)
        {
          TempData["Message"] = $"Transaction id {Id} was not found";
          return RedirectToPage("Search");
        }

        customer = _sqlDb.Customers_GetByTransactionId(Id);
        FullName = customer.FullName.Contains("Guest") ? "" : customer.FullName;

        GenerateRefundReceiptData();
        return Page();
      }

      TempData["Message"] = "Something went wrong!";
      return RedirectToPage("Search");
    }

    private void GenerateRefundReceiptData()
    {
      CultureInfo dollar = new CultureInfo("en-US");
      RefundItems = _sqlDb.RefundLines_GetByTransactionId(Id).Sum(t => t.Qty);

      TransactionModel transaction = _sqlDb.Transactions_GetById(Id);
      Created = transaction.Created.ToString("yyyy/MM/dd");
      RefundTotal = transaction.Amount.ToString("C2", dollar);
      RefundMethod = transaction.Method.ToString();
    }
  }
}
