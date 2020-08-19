using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ChannelAdvisorLibrary;
using ChannelAdvisorLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;
using NewAgePOSLibrary.Models;

namespace NewAgePOS.Pages
{
  public class CartModel : PageModel
  {
    private readonly IChannelAdvisor _ca;
    private readonly ISQLData _sqlDb;

    public CartModel(IChannelAdvisor ca, ISQLData sqlDb)
    {
      _ca = ca;
      _sqlDb = sqlDb;
    }

    [BindProperty]
    [Display(Name = "SKUs or UPCs")]
    public string Codes { get; set; }

    [BindProperty(SupportsGet = true)]
    public List<SaleLineModel> SaleLines { get; set; }

    [BindProperty(SupportsGet = true)]
    public int SaleId { get; set; }

    [BindProperty(SupportsGet = true)]
    public float TaxPct { get; set; }


    public void OnGet()
    {
      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      TaxPct = _sqlDb.Taxes_GetBySaleId(SaleId);

      SaleLines.ForEach(s => s.LineTotal = (s.Price - s.DiscAmt) * (1 - s.DiscPct / 100f) * s.Qty);
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
      if (string.IsNullOrEmpty(Codes)) return RedirectToPage();

      List<string> productCodes = Codes.Trim().Replace(" ", string.Empty).Split(Environment.NewLine).ToList();
      Dictionary<string, int> uniqueCodes = new Dictionary<string, int>();

      List<IGrouping<string, string>> groupedCodes = productCodes.GroupBy(p => p).ToList();
      foreach (var code in groupedCodes)
      {
        SaleLineModel saleLine = new SaleLineModel();
        if (code.Key.Contains("_"))
          saleLine = SaleLines.FirstOrDefault(s => s.Sku == code.Key);
        else
          saleLine = SaleLines.FirstOrDefault(s => s.Upc == code.Key);

        if (saleLine != null)
        {
          saleLine.Qty += code.Count();
          saleLine.IsUpdated = true;
          _sqlDb.SaleLines_Update(saleLine.Id, saleLine.Qty, saleLine.DiscAmt, saleLine.DiscPct);
          continue;
        }

        uniqueCodes.Add(code.Key, code.Count());
      }

      List<ProductModel> products = new List<ProductModel>();
      products.AddRange(await _ca.GetProductsByCodeAsync(uniqueCodes.Select(u => u.Key).ToList()));
      foreach (var product in products)
      {
        int productId = _sqlDb.Products_GetByValues(product.Sku, product.Upc, product.Cost, product.Price, product.AllName);

        int qty1 = uniqueCodes.ContainsKey(product.Sku) ? uniqueCodes[product.Sku] : 0;
        int qty2 = uniqueCodes.ContainsKey(product.Upc) ? uniqueCodes[product.Upc] : 0;

        int qty = qty1 + qty2;

        _sqlDb.SaleLines_Insert(SaleId, productId, qty);
      }

      return RedirectToPage();
    }

    public IActionResult OnPostUpdate()
    {
      if (!ModelState.IsValid) return Page();

      for (int i = SaleLines.Count - 1; i >= 0; i--)
      {
        if (SaleLines[i].Qty == 0)
        {
          _sqlDb.SaleLines_Delete(SaleLines[i].Id);
          SaleLines.RemoveAt(i);
          continue;
        }

        if (!SaleLines[i].IsUpdated)
        {
          _sqlDb.SaleLines_Update(SaleLines[i].Id, SaleLines[i].Qty, SaleLines[i].DiscAmt, SaleLines[i].DiscPct);
        }
      }

      return RedirectToPage();
    }
  }
}
