using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ChannelAdvisorLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Internal;
using NewAgePOS.Utilities;
using NewAgePOS.ViewModels.Sale;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;
using Newtonsoft.Json.Linq;

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

    [BindProperty(SupportsGet = true)]
    public int SaleId { get; set; }

    [BindProperty]
    public CartViewModel CartVM { get; set; }

    public IActionResult OnGet()
    {
      SaleModel sale = _sqlDb.Sales_GetById(SaleId);
      if (sale == null)
      {
        TempData["Message"] = "Invalid Sale Id";
        return RedirectToPage("Search");
      }

      bool isComplete = sale.IsComplete;
      if (isComplete)
      {
        TempData["Message"] = $"Cannot access Cart because Sale Id { SaleId } was completed.";
        return RedirectToPage("Search");
      }

      return Page();
    }

    public IActionResult OnPostApplyDiscount(int saleLineId, int qty, float discPct)
    {
      if (discPct > 100)
      {
        TempData["Message"] = "Discount percent can not be greater than 100";
        return Page();
      }

      _sqlDb.SaleLines_Update(saleLineId, qty, discPct);

      return RedirectToPage();
    }

    #region Products
    public async Task<IActionResult> OnPostAddProductsAsync()
    {
      if (string.IsNullOrEmpty(CartVM.Codes)) return RedirectToPage();

      List<string> productCodes = CartVM.Codes.Trim()
        .Replace(" ", string.Empty)
        .Split(Environment.NewLine)
        .ToList();

      Dictionary<string, int> codesWithQty = new Dictionary<string, int>();
      await Task.Run(() => productCodes.ForEach(p =>
      {
        if (codesWithQty.TryGetValue(p, out int qty))
        {
          codesWithQty[p]++;
        }
        else
        {
          codesWithQty.Add(p, 1);
        }
      }));

      return new JsonResult(codesWithQty);

      //await CheckAndUpdateExistingLines(productCodes, codesWithQty);

      //List<JObject> jObjects = await _ca.GetProductsByCodeAsync(codesWithQty.Select(c => c.Key).ToList());
      
      //await Task.Run(() => CreateSaleLines(codesWithQty, jObjects));

      //if (codesWithQty.Count > 0)
      //{
      //  string notFoundCodes = string.Join(", ", codesWithQty.Select(c => c.Key));
      //  TempData["Message"] = $"Was not able to find { notFoundCodes }";
      //}

      //return RedirectToPage();
    }

    private void CreateSaleLines(Dictionary<string, int> codesWithQty, List<JObject> jObjects)
    {
      foreach (var item in jObjects)
      {
        string sku, upc;
        int productId;
        CheckProductAgainstDB(item, out sku, out upc, out productId);

        if (codesWithQty.TryGetValue(sku, out int qty1))
          codesWithQty.Remove(sku);

        if (codesWithQty.TryGetValue(upc, out int qty2))
          codesWithQty.Remove(upc);

        _sqlDb.SaleLines_Insert(SaleId, productId, null, qty1 + qty2);
      }
    }

    private void CheckProductAgainstDB(JObject item, out string sku, out string upc, out int productId)
    {
      sku = item[CAStrings.sku].ToString();
      upc = item[CAStrings.upc].ToString();
      float cost = string.IsNullOrEmpty(item[CAStrings.cost].ToString()) ? 0 :
        item[CAStrings.cost].ToObject<float>();
      float price = item[CAStrings.attributes]
        .FirstOrDefault(i => i[CAStrings.name]
          .ToString() == CAStrings.bcprice)[CAStrings.Value]
        .ToObject<float>();
      string allName = item[CAStrings.attributes]
        .FirstOrDefault(i => i[CAStrings.name]
          .ToString() == CAStrings.allName)[CAStrings.Value]
        .ToString();

      productId = 0;
      ProductModel product = _sqlDb.Products_GetByCode(sku, upc);

      if (product == null)
        productId = _sqlDb.Products_Insert(sku, upc, cost, price, allName);
      else if (product.Cost != cost || product.Price != price || product.AllName != allName)
        _sqlDb.Products_Update(product.Id, cost, price, allName);
      else
        productId = product.Id;
    }

    private async Task CheckAndUpdateExistingLines(List<string> productCodes, Dictionary<string, int> codesWithQty)
    {
      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      List<ProductModel> products = _sqlDb.Products_GetBySaleId(SaleId);
      List<Task> tasks = new List<Task>();

      foreach (var productCode in productCodes.Distinct())
      {
        int qtyToAdd = 0;
        ProductModel product = new ProductModel();
        SaleLineModel saleLine = new SaleLineModel();

        if (productCode.Contains("_"))
          product = products.FirstOrDefault(p => p.Sku.ToLower() == productCode.ToLower());
        else
          product = products.FirstOrDefault(p => p.Upc == productCode);

        if (product != null)
        {
          codesWithQty.TryGetValue(productCode, out qtyToAdd);
          codesWithQty.Remove(productCode);

          saleLine = saleLines.First(sl => sl.ProductId == product.Id);
          saleLine.Qty += qtyToAdd;
          tasks.Add(Task.Run(() => _sqlDb.SaleLines_Update(saleLine.Id, saleLine.Qty, saleLine.DiscPct)));
        }
      }

      await Task.WhenAll(tasks);
    }

    public IActionResult OnPostRemoveProducts()
    {
      if (string.IsNullOrEmpty(CartVM.Codes)) return RedirectToPage();

      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      List<ProductModel> products = _sqlDb.Products_GetBySaleId(SaleId);

      List<string> productCodes = CartVM.Codes
        .Trim()
        .Replace(" ", string.Empty)
        .Split(Environment.NewLine)
        .ToList();

      List<IGrouping<string, string>> groupedCodes = productCodes
        .GroupBy(p => p)
        .ToList();

      for (int i = groupedCodes.Count - 1; i >= 0; i--)
      {
        ProductModel product = new ProductModel();

        if (groupedCodes[i].Key.Contains("_"))
          product = products.FirstOrDefault(p => p.Sku.ToLower() == groupedCodes[i].Key.ToLower());
        else
          product = products.FirstOrDefault(p => p.Upc == groupedCodes[i].Key);

        if (product != null)
        {
          SaleLineModel saleLine = saleLines.FirstOrDefault(s => s.ProductId.Value == product.Id);
          saleLine.Qty -= groupedCodes[i].Count();

          if (saleLine.Qty <= 0)
          {
            _sqlDb.SaleLines_Delete(saleLines[i].Id);
            continue;
          }

          _sqlDb.SaleLines_Update(saleLine.Id, saleLine.Qty, saleLine.DiscPct);
          groupedCodes.RemoveAt(i);
        }
      }

      if (groupedCodes.Count > 0)
      {
        string notFoundCodes = string.Join(", ", groupedCodes.Select(g => g.Key));
        TempData["Message"] = $"Unable to find { notFoundCodes } in cart";
      }

      return RedirectToPage();
    }
    #endregion

    #region Gift Cards
    public IActionResult OnPostAddGiftCards()
    {
      if (!ModelState.IsValid) return Page();

      List<string> msgs = new List<string>();
      List<string> giftCardCodes = CartVM.GiftCardCodes
        .Trim()
        .Replace(" ", string.Empty)
        .Split(Environment.NewLine)
        .Distinct()
        .ToList();

      foreach (string code in giftCardCodes)
      {
        GiftCardModel gc = _sqlDb.GiftCards_GetByCode(code);

        if (gc != null)
        {
          msgs.Add($"{ code } already exists. Skipped");
          continue;
        }

        int giftCardId = _sqlDb.GiftCards_Insert(code, CartVM.GiftCardAmount);

        _sqlDb.SaleLines_Insert(SaleId, null, giftCardId, 1);
      }

      if (msgs.Count > 0) TempData["Message"] = string.Join(Environment.NewLine, msgs);

      return RedirectToPage();
    }

    public IActionResult OnPostRemoveGiftCards()
    {
      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      List<GiftCardModel> giftCards = _sqlDb.GiftCards_GetBySaleId(SaleId);
      List<string> msgs = new List<string>();
      List<string> giftCardCodes = CartVM.GiftCardCodes
        .Trim()
        .Replace(" ", string.Empty)
        .Split(Environment.NewLine)
        .Distinct()
        .ToList();

      foreach (string code in giftCardCodes)
      {
        GiftCardModel gc = giftCards.FirstOrDefault(g => g.Code == code);

        if (gc == null)
        {
          msgs.Add($"{ code } was not found in the cart");
          continue;
        }

        int saleLineId = saleLines.FirstOrDefault(s => s.GiftCardId.HasValue && s.GiftCardId.Value == gc.Id).Id;
        _sqlDb.SaleLines_Delete(saleLineId);
        _sqlDb.GiftCards_Delete(gc.Id);
      }

      if (msgs.Count > 0) TempData["Message"] = string.Join(Environment.NewLine, msgs);

      return RedirectToPage();
    }
    #endregion

    public IActionResult OnPostProceed()
    {
      if (_sqlDb.SaleLines_GetBySaleId(SaleId).Any())
        return RedirectToPage("Guest", new { SaleId });

      TempData["Message"] = "There are no items to proceed with";
      return RedirectToPage();
    }

    #region Trade Ins
    public IActionResult OnPostAddTradeIn()
    {
      if (!ModelState.IsValid) return Page();

      _sqlDb.SaleLines_Insert(SaleId, CartVM.TradeInValue, CartVM.TradeInQty);

      return RedirectToPage();
    }

    public IActionResult OnPostRemoveTradeIn()
    {
      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      SaleLineModel saleLineToDelete = saleLines.FirstOrDefault(sl => sl.Id == CartVM.SaleLineId);

      if (saleLineToDelete == null)
      {
        TempData["Message"] = $"{CartVM.SaleLineId} does not exist";
        return RedirectToPage();
      }

      _sqlDb.SaleLines_Delete(saleLineToDelete.Id);

      TempData["Message"] = $"{CartVM.SaleLineId} has been removed";
      return RedirectToPage();
    }
    #endregion
  }
}
