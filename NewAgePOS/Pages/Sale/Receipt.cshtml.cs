using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
      CustomerModel customer = _sqlDb.Customers_GetBySaleId(Id);
      FullName = customer.FullName.Contains("Guest") ? "" : customer.FullName;

      if (IdType == "Sale")
      {
        bool isComplete = _sqlDb.Sales_GetById(Id).IsComplete;
        if (!isComplete)
        {
          TempData["Message"] = $"Sale id {Id} is not complete yet";
          return RedirectToPage("Search");
        }

        Created = _sqlDb.Sales_GetById(Id).Created.ToString("yyyy/MM/dd");
        return Page();
      }
      else if(IdType == "Refund")
      {
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
      RefundMethod = transaction.Method;
    }
  }
}
