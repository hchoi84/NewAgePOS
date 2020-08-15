using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ChannelAdvisorLibrary;
using ChannelAdvisorLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOS.Utilities;
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
    public int? SaleId { get; set; }

    public void OnGet()
    {
      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId.Value);
    }

    public async Task<IActionResult> OnPost()
    {
      if (!ModelState.IsValid)
      {
        return Page();
      }

      if (!string.IsNullOrEmpty(Codes))
      {
        List<string> productCodes = Codes.Trim().Replace(" ", string.Empty).Split(Environment.NewLine).ToList();
        Dictionary<string, int> uniqueCodes = new Dictionary<string, int>();

        // Edit existing. Code exists in SaleLines
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
            // Update saleLine to DB
            _sqlDb.SaleLines_Update(saleLine.Id, saleLine.Qty, saleLine.DiscAmt, saleLine.DiscPct);
            continue;
          }
          uniqueCodes.Add(code.Key, code.Count());
        }

        // Create new. Code doesnt exist in SaleLines
        List<ProductModel> products = new List<ProductModel>();
        products.AddRange(await _ca.GetProductsByCodeAsync(uniqueCodes.Select(u => u.Key).ToList()));
        foreach (var product in products)
        {
          // Check if product info exists in DB
          // If so, retrieve Products Id and insert SaleLines
          // If not, insert, retrieve Products Id, and insert SaleLines
          int productId = _sqlDb.Products_GetByValues(product.Sku, product.Upc, product.Cost, product.Price, product.AllName);
          KeyValuePair<string, int> code = uniqueCodes.FirstOrDefault(u => u.Key == product.Sku || u.Key == product.Upc);
          _sqlDb.SaleLines_Insert(SaleId.Value, productId, code.Value, 0, 0, 0);
        }
      }

      for (int i = SaleLines.Count - 1; i >= 0; i--)
      {
        if (SaleLines[i].Qty == 0)
        {
          // Delete from DB
          _sqlDb.SaleLines_Delete(SaleLines[i].Id);
          SaleLines.RemoveAt(i);
          continue;
        }

        // Update to DB
        if (!SaleLines[i].IsUpdated)
        {
          _sqlDb.SaleLines_Update(SaleLines[i].Id, SaleLines[i].Qty, SaleLines[i].DiscAmt, SaleLines[i].DiscPct);
        }
      }

      return RedirectToPage();
    }

    private async Task<IActionResult> TempArchive()
    {
      List<ProductModel> Products = new List<ProductModel>();

      // For OnPost()
      if (!string.IsNullOrEmpty(Codes))
      {
        List<string> productCodes = Codes.Trim().Replace(" ", string.Empty).Split(Environment.NewLine).ToList();

        Dictionary<string, int> uniqueCodes = new Dictionary<string, int>();

        for (int i = productCodes.Count - 1; i >= 0; i--)
        {
          ProductModel p = new ProductModel();

          if (productCodes[i].Contains("_"))
            p = Products.FirstOrDefault(p => p.Sku == productCodes[i]);
          else
            p = Products.FirstOrDefault(p => p.Upc == productCodes[i]);

          if (p != null)
          {
            p.Qty++;
            productCodes.RemoveAt(i);
            continue;
          }

          if (uniqueCodes.ContainsKey(productCodes[i]))
            uniqueCodes[productCodes[i]]++;
          else
            uniqueCodes.Add(productCodes[i], 1);
        }

        if (uniqueCodes.Count > 0)
        {
          List<string> codes = uniqueCodes.Select(u => u.Key).ToList();
          Products.AddRange(await _ca.GetProductsByCodeAsync(codes));

          foreach (KeyValuePair<string, int> uniqueCode in uniqueCodes)
          {
            if (uniqueCode.Value == 1) continue;

            ProductModel p = new ProductModel();

            if (uniqueCode.Key.Contains("_"))
              p = Products.FirstOrDefault(p => p.Sku == uniqueCode.Key);
            else
              p = Products.FirstOrDefault(p => p.Upc == uniqueCode.Key);

            if (p != null) p.Qty = uniqueCode.Value;
          }
        }
      }

      List<ProductModel> prodsToDel = Products.Where(p => p.Qty == 0).ToList();
      foreach (var prodToDel in prodsToDel)
      {
        Products.Remove(prodToDel);
      }

      HttpContext.Session.SetObject("Products", Products);

      return RedirectToPage();
    }
  }
}
