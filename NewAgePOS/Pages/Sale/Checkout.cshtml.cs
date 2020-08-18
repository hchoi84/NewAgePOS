using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;
using NewAgePOSLibrary.Models;

namespace NewAgePOS.Pages.Sale
{
  public class CheckoutModel : PageModel
  {
    private readonly ISQLData _sqlDb;

    public CheckoutModel(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
    }

    [BindProperty(SupportsGet = true)]
    public int SaleId { get; set; }

    [BindProperty(SupportsGet = true)]
    public List<SaleLineModel> SaleLines { get; set; }

    [BindProperty(SupportsGet = true)]
    public float TaxPct { get; set; }

    [BindProperty(SupportsGet = true)]
    public CustomerModel Customer { get; set; }

    public void OnGet()
    {
      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      SaleLines.ForEach(s => s.LineTotal = (s.Price - s.DiscAmt) - (1 - TaxPct / 100) * s.Qty);

      TaxPct = _sqlDb.Taxes_GetBySaleId(SaleId);

      Customer = _sqlDb.Customers_GetBySaleId(SaleId);
      if (Customer != null)
      {
        TextInfo ti = new CultureInfo("en-US", false).TextInfo;

        Customer.FirstName = ti.ToTitleCase(Customer.FirstName);
        Customer.LastName = ti.ToTitleCase(Customer.LastName);
      }
    }
  }
}
