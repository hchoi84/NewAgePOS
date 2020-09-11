using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public int SaleId { get; set; }

    public string Created { get; set; }

    public string FullName { get; set; }

    public void OnGet()
    {
      CustomerModel customer = _sqlDb.Customers_GetBySaleId(SaleId);
      FullName = customer.FullName.Contains("Guest") ? "" : customer.FullName;

      Created = _sqlDb.Sales_GetById(SaleId).Created.ToString("yyyy/MM/dd");
    }
  }
}
