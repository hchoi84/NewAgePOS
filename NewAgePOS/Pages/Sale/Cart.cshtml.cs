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

    #region Products
    public async Task<IActionResult> OnPostAddProductsAsync()
    {
      if (string.IsNullOrEmpty(CartVM.ProductCodes))
      {
        TempData["Message"] = "Codes can not be blank. Please enter at least one SKU or UPC";
        return RedirectToPage();
      }

      Dictionary<string, int> codeCount = CartVM.ProductCodes.CountIt();
      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);

      if (!saleLines.Any())
      {
        IEnumerable<ProductModel> products = await GetFromCaAsync(codeCount.Select(c => c.Key));
        AddProductsToDb(products);
        AddSaleLinesToDb(products, codeCount);
      }
      else
      {
        List<ProductModel> productModels = _sqlDb.Products_GetBySaleId(SaleId);

        foreach (var code in codeCount)
        {
          ProductModel p = productModels.FirstOrDefault(p => p.Sku == code.Key || p.Upc == code.Key);
          if (p == null) continue;

          SaleLineModel s = saleLines.FirstOrDefault(s => s.ProductId == p.Id);
          s.Qty += code.Value;
          _sqlDb.SaleLines_Update(s.Id, s.Qty, s.DiscPct);
          codeCount.Remove(code.Key);
        }
      }

      if (codeCount.Count > 0)
      {
        IEnumerable<ProductModel> products = await GetFromCaAsync(codeCount.Select(c => c.Key));
        AddProductsToDb(products);
        AddSaleLinesToDb(products, codeCount);
      }

      if (codeCount.Count > 0)
      {
        string notFoundCodes = string.Join(", ", codeCount.Select(c => c.Key));
        TempData["Message"] = $"Was not able to find { notFoundCodes } in CA";
      }

      return RedirectToPage();
    }

    private async Task<IEnumerable<ProductModel>> GetFromCaAsync(IEnumerable<string> codes)
    {
      IEnumerable<JObject> items = await _ca.GetProductsByCodeAsync(codes);
      List<ProductModel> products = new List<ProductModel>();

      foreach (JObject item in items)
      {
        ProductModel product = new ProductModel();
        product.Sku = item[CAStrings.sku].ToString();
        product.Upc = item[CAStrings.upc].ToString();
        product.Cost = string.IsNullOrEmpty(item[CAStrings.cost].ToString()) ? 0
          : item[CAStrings.cost].ToObject<float>();
        product.Price = item[CAStrings.attributes]
          .FirstOrDefault(i => i[CAStrings.name]
            .ToString() == CAStrings.bcprice)[CAStrings.Value]
          .ToObject<float>();
        product.AllName = item[CAStrings.attributes]
          .FirstOrDefault(i => i[CAStrings.name]
            .ToString() == CAStrings.allName)[CAStrings.Value]
          .ToString();

        products.Add(product);
      }

      return products;
    }

    private void AddProductsToDb(IEnumerable<ProductModel> products)
    {
      foreach (var product in products)
      {
        ProductModel p = _sqlDb.Products_GetByCode(product.Sku, product.Upc);
        if (p == null)
        {
          int id = _sqlDb.Products_Insert(product.Sku, product.Upc, product.Cost, product.Price, product.AllName);
          product.Id = id;
          continue;
        }

        product.Id = p.Id;
        if (product.Sku != p.Sku || product.Upc != p.Upc || product.Cost != p.Cost || product.Price != p.Price || product.AllName != p.AllName)
        {
          _sqlDb.Products_Update(product);
        }
      }
    }

    private void AddSaleLinesToDb(IEnumerable<ProductModel> products, Dictionary<string, int> codeCount)
    {
      foreach (var product in products)
      {
        if (!codeCount.TryGetValue(product.Sku, out int qty1)) qty1 = 0;
        if (!codeCount.TryGetValue(product.Upc, out int qty2)) qty2 = 0;

        _sqlDb.SaleLines_Insert(SaleId, product.Id, null, qty1 + qty2);

        if (qty1 > 0) codeCount.Remove(product.Sku);
        if (qty2 > 0) codeCount.Remove(product.Upc);
      }
    }

    public IActionResult OnPostRemoveProducts()
    {
      if (string.IsNullOrEmpty(CartVM.ProductCodes))
      {
        TempData["Message"] = "Codes can not be blank. Please enter at least one SKU or UPC";
        return RedirectToPage();
      }

      IEnumerable<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      if (saleLines == null)
      {
        TempData["Message"] = "There are no items in the cart to update";
        return RedirectToPage();
      }

      Dictionary<string, int> codeCount = CartVM.ProductCodes.CountIt();
      IEnumerable<ProductModel> products = _sqlDb.Products_GetBySaleId(SaleId);

      foreach (var code in codeCount)
      {
        ProductModel product = products.FirstOrDefault(p => p.Sku.ToUpper() == code.Key || p.Upc == code.Key);
        if (product == null) continue;

        SaleLineModel saleLine = saleLines.FirstOrDefault(s => s.ProductId == product.Id);
        saleLine.Qty -= code.Value;

        if (saleLine.Qty <= 0) _sqlDb.SaleLines_Delete(saleLine.Id);
        else _sqlDb.SaleLines_Update(saleLine.Id, saleLine.Qty, saleLine.DiscPct);

        codeCount.Remove(code.Key);
      }

      if (codeCount.Count > 0)
      {
        string notFoundCodes = string.Join(", ", codeCount.Select(c => c.Key));
        TempData["Message"] = $"Was not able to find { notFoundCodes } in the Cart";
      }

      return RedirectToPage();
    }
    #endregion

    public IActionResult OnPostApplyDiscount(int saleLineId, int qty, float discPct)
    {
      if (discPct < 0 || discPct > 100)
      {
        TempData["Message"] = "Discount percent must be between 0 and 100";
        return Page();
      }

      _sqlDb.SaleLines_Update(saleLineId, qty, discPct);

      return RedirectToPage();
    }

    #region Gift Cards
    public IActionResult OnPostAddGiftCards()
    {
      List<string> errorMsgs = new List<string>();

      if (String.IsNullOrEmpty(CartVM.GiftCardCodes) || CartVM.GiftCardAmount == 0 || CartVM.GiftCardAmountConfirm == 0) 
        errorMsgs.Add("All Gift Card fields are required");
      else if (CartVM.GiftCardAmount <= 0) 
        errorMsgs.Add("Gift Card Amount must be greater than 0");
      else if (CartVM.GiftCardAmount != CartVM.GiftCardAmountConfirm) 
        errorMsgs.Add("Gift Card Amount does not match with the confirmation amount");

      if (errorMsgs.Any())
      {
        TempData["Message"] = String.Join(Environment.NewLine, errorMsgs);
        return Page();
      }

      Dictionary<string, int> codeCounts = CartVM.GiftCardCodes.CountIt();

      foreach (var code in codeCounts)
      {
        if (code.Key.Length != 13)
        {
          errorMsgs.Add($"{ code.Key }: Invalid Gift Card code. Must be 13 digits");
          continue;
        }

        bool gcIsInDB = _sqlDb.GiftCards_GetByCode(code.Key) != null;

        if (gcIsInDB)
        {
          errorMsgs.Add($"{ code.Key }: Same code already exists");
          continue;
        }

        int gcId = _sqlDb.GiftCards_Insert(code.Key, CartVM.GiftCardAmount);
        _sqlDb.SaleLines_Insert(SaleId, null, gcId, 1);
      }

      if (errorMsgs.Any())
        TempData["Message"] = String.Join(Environment.NewLine, errorMsgs);

      return RedirectToPage();
    }

    public IActionResult OnPostRemoveGiftCards()
    {
      List<string> errorMsgs = new List<string>();

      if (String.IsNullOrEmpty(CartVM.GiftCardCodes))
        errorMsgs.Add("Gift Card Codes field is required");

      if (errorMsgs.Any())
      {
        TempData["Message"] = String.Join(Environment.NewLine, errorMsgs);
        return Page();
      }

      Dictionary<string, int> codeCounts = CartVM.GiftCardCodes.CountIt();
      IEnumerable<GiftCardModel> giftCards = _sqlDb.GiftCards_GetBySaleId(SaleId);
      IEnumerable<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);

      foreach (var code in codeCounts)
      {
        if (code.Key.Length != 13)
        {
          errorMsgs.Add($"{ code.Key }: Invalid Gift Card code. Must be 13 digits");
          continue;
        }

        GiftCardModel gc = giftCards.FirstOrDefault(gc => gc.Code == code.Key);

        if (gc == null)
        {
          errorMsgs.Add($"{ code.Key }: Does not exist in current sales");
          continue;
        }

        SaleLineModel saleLine = saleLines.FirstOrDefault(sl => sl.GiftCardId == gc.Id);

        _sqlDb.SaleLines_Delete(saleLine.Id);
        _sqlDb.GiftCards_Delete(gc.Id);
      }

      if (errorMsgs.Any())
        TempData["Message"] = String.Join(Environment.NewLine, errorMsgs);

      return RedirectToPage();
    }
    #endregion

    #region Trade Ins
    public IActionResult OnPostAddTradeIn()
    {
      List<string> errorMsgs = new List<string>();

      if (CartVM.TradeInValue == 0 || CartVM.ConfirmTradeInValue == 0 || CartVM.TradeInQty == 0)
        errorMsgs.Add("All Trade In fields are required");
      else if (CartVM.TradeInValue != CartVM.ConfirmTradeInValue)
        errorMsgs.Add("Trade In Value and the Confirm Value does not match");
      else if (CartVM.TradeInValue <= 0)
        errorMsgs.Add("Trade In Value must be greater than 0");
      else if (CartVM.TradeInQty <= 0)
        errorMsgs.Add("Trade In Quantity must be greater than 0");

      if (errorMsgs.Any())
      {
        TempData["Message"] = string.Join(Environment.NewLine, errorMsgs);
        return Page();
      }

      _sqlDb.SaleLines_Insert(SaleId, CartVM.TradeInValue, CartVM.TradeInQty);

      return RedirectToPage();
    }

    public IActionResult OnPostRemoveTradeIn()
    {
      if (CartVM.SaleLineId == 0)
      {
        TempData["Message"] = "Trade In ID field is required";
        return Page();
      }

      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      SaleLineModel saleLineToDelete = saleLines.FirstOrDefault(sl => sl.Id == CartVM.SaleLineId);

      _sqlDb.SaleLines_Delete(saleLineToDelete.Id);

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
  }
}
