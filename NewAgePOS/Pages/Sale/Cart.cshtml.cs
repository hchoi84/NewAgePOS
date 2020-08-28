using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ChannelAdvisorLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;

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

    [BindProperty]
    public List<SaleLineModel> SaleLines { get; set; }

    [BindProperty(SupportsGet = true)]
    public int SaleId { get; set; }

    [BindProperty]
    public float TaxPct { get; set; }


    public IActionResult OnGet()
    {
      bool isComplete = _sqlDb.Sales_GetStatus(SaleId);
      if (isComplete)
      {
        TempData["Message"] = $"Cannot access Cart because Sale Id { SaleId } was completed.";
        return RedirectToPage("Index");
      }

      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      TaxPct = _sqlDb.Taxes_GetBySaleId(SaleId);

      return Page();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
      if (string.IsNullOrEmpty(Codes)) return RedirectToPage();

      List<string> productCodes = Codes.Trim().Replace(" ", string.Empty).Split(Environment.NewLine).ToList();
      List<IGrouping<string, string>> groupedCodes = productCodes.GroupBy(p => p).ToList();

      UpdateCart(groupedCodes);

      if (groupedCodes.Count > 0) await GetFromChannelAdvisor(groupedCodes);
      if (groupedCodes.Count > 0) GetFromDb(groupedCodes);
      if (groupedCodes.Count > 0)
      {
        string notFoundCodes = string.Join(", ", groupedCodes.Select(g => g.Key));
        TempData["Message"] = $"Unable to find { notFoundCodes } in cart, ChannelAdvisor, and manually entered product record";
      }

      return RedirectToPage();
    }

    private void UpdateCart(List<IGrouping<string, string>> groupedCodes)
    {
      for (int i = groupedCodes.Count - 1; i >= 0; i--)
      {
        SaleLineModel saleLine = new SaleLineModel();
        if (groupedCodes[i].Key.Contains("_"))
          saleLine = SaleLines.FirstOrDefault(s => s.Sku == groupedCodes[i].Key);
        else
          saleLine = SaleLines.FirstOrDefault(s => s.Upc == groupedCodes[i].Key);

        if (saleLine != null)
        {
          saleLine.Qty += groupedCodes[i].Count();
          saleLine.IsUpdated = true;
          _sqlDb.SaleLines_Update(saleLine.Id, saleLine.Qty, saleLine.DiscAmt, saleLine.DiscPct);
          groupedCodes.RemoveAt(i);
        }
      }
    }

    private async Task GetFromChannelAdvisor(List<IGrouping<string, string>> groupedCodes)
    {
      List<ProductModel> products = new List<ProductModel>();
      Dictionary<string, int> uniqueCodes = uniqueCodes = groupedCodes
        .ToDictionary(g => g.Key, g => g.Count(), StringComparer.InvariantCultureIgnoreCase);
      products.AddRange(await _ca.GetProductsByCodeAsync(groupedCodes.Select(g => g.Key).ToList()));
      foreach (var product in products)
      {
        int productId = 0;
        ProductModel productDb = _sqlDb.Products_GetByCode(product.Sku, product.Upc);
        if (productDb == null)
          productId = _sqlDb.Products_Insert(product.Sku, product.Upc, product.Cost, product.Price, product.AllName);
        else if (productDb.Cost != product.Cost || productDb.Price != product.Price || productDb.AllName != product.AllName)
          _sqlDb.Products_Update(productDb.Id, product.Cost, product.Price, product.AllName);
        else
          productId = productDb.Id;

        int qty1 = 0;
        int qty2 = 0;

        if (uniqueCodes.ContainsKey(product.Sku))
        {
          qty1 = uniqueCodes[product.Sku];
          groupedCodes.Remove(groupedCodes.FirstOrDefault(g => g.Key == product.Sku.ToLower() || g.Key == product.Sku.ToUpper()));
        }

        if (uniqueCodes.ContainsKey(product.Upc))
        {
          qty2 = uniqueCodes[product.Upc];
          groupedCodes.Remove(groupedCodes.FirstOrDefault(g => g.Key == product.Upc));
        }

        int qty = qty1 + qty2;

        _sqlDb.SaleLines_Insert(SaleId, productId, qty);
      }
    }

    private void GetFromDb(List<IGrouping<string, string>> groupedCodes)
    {
      for (int i = groupedCodes.Count - 1; i >= 0; i--)
      {
        int productId;
        if (groupedCodes[i].Key.Contains("_"))
          productId = _sqlDb.Products_GetByCode(groupedCodes[i].Key, "").Id;
        else
          productId = _sqlDb.Products_GetByCode("", groupedCodes[i].Key).Id;

        if (productId > 0)
        {
          int qty = groupedCodes[i].Count();
          _sqlDb.SaleLines_Insert(SaleId, productId, qty);
          groupedCodes.RemoveAt(i);
        }
      }
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
