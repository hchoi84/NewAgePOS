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
      if (SaleId.HasValue)
      {
        SaleLines = _sqlDb.GetSaleLinesBySaleId(SaleId.Value);
      }
      else
      {
        SaleId = _sqlDb.CreateSale();
      }
    }

    public IActionResult OnPost()
    {
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
