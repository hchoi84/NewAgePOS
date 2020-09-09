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
    private readonly IShare _share;

    public CartModel(IChannelAdvisor ca, ISQLData sqlDb, IShare share)
    {
      _ca = ca;
      _sqlDb = sqlDb;
      _share = share;
    }

    [BindProperty(SupportsGet = true)]
    public int SaleId { get; set; }

    [BindProperty]
    [Display(Name = "SKUs or UPCs")]
    public string Codes { get; set; }

    [BindProperty]
    public List<CartDiscModel> CartDiscs { get; set; }

    [BindProperty]
    public string GiftCardCodes { get; set; }

    [BindProperty]
    public float GiftCardAmount { get; set; }

    public List<ItemListViewModel> Items { get; set; }

    public IActionResult OnGet()
    {
      bool isComplete = _sqlDb.Sales_GetById(SaleId).IsComplete;

      if (isComplete)
      {
        TempData["Message"] = $"Cannot access Cart because Sale Id { SaleId } was completed.";
        return RedirectToPage("Search");
      }

      List<ItemListViewModel> items = _share.GenerateCartViewModel(SaleId);
      Items = items.OrderBy(i => i.Sku).ToList();

      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);

      CartDiscs = new List<CartDiscModel>();
      foreach (var item in Items)
      {
        CartDiscs.Add(new CartDiscModel()
        {
          SaleLineId = item.SaleLineId,
          DiscPct = saleLines.FirstOrDefault(s => s.Id == item.SaleLineId).DiscPct
        });
      }

      return Page();
    }

    public IActionResult OnPostApplyDiscount()
    {
      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);

      foreach (CartDiscModel cartDisc in CartDiscs)
      {
        SaleLineModel saleLine = saleLines.FirstOrDefault(sl => sl.Id == cartDisc.SaleLineId);

        if (cartDisc.DiscPct > 100)
        {
          ModelState.AddModelError(string.Empty, "Discount percent can not be greater than 100");
          return Page();
        }

        if (saleLine.DiscPct != cartDisc.DiscPct)
          _sqlDb.SaleLines_Update(cartDisc.SaleLineId, saleLine.Qty, cartDisc.DiscPct);
      }

      return RedirectToPage();
    }

    #region Add Products
    public async Task<IActionResult> OnPostAddProductsAsync()
    {
      if (string.IsNullOrEmpty(Codes)) return RedirectToPage();

      List<string> productCodes = Codes.Trim()
        .Replace(" ", string.Empty)
        .Split(Environment.NewLine)
        .ToList();

      List<IGrouping<string, string>> groupedCodes = productCodes.GroupBy(p => p).ToList();

      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      List<ProductModel> products = _sqlDb.Products_GetBySaleId(SaleId);

      UpdateCart(groupedCodes, saleLines, products);
      if (groupedCodes.Count > 0) await GetFromChannelAdvisor(groupedCodes);
      if (groupedCodes.Count > 0)
      {
        string notFoundCodes = string.Join(", ", groupedCodes.Select(g => g.Key));
        TempData["Message"] = $"Unable to find { notFoundCodes } in cart, ChannelAdvisor, and manually entered product record";
      }

      return RedirectToPage();
    }

    private void UpdateCart(List<IGrouping<string, string>> groupedCodes, List<SaleLineModel> saleLines, List<ProductModel> products)
    {
      for (int i = groupedCodes.Count - 1; i >= 0; i--)
      {
        ProductModel product = new ProductModel();
        SaleLineModel saleLine = new SaleLineModel();

        if (groupedCodes[i].Key.Contains("_"))
          product = products.FirstOrDefault(p => p.Sku.ToLower() == groupedCodes[i].Key.ToLower());
        else
          product = products.FirstOrDefault(p => p.Upc == groupedCodes[i].Key);

        if (product != null)
        {
          saleLine = saleLines.FirstOrDefault(s => s.ProductId.Value == product.Id);
          saleLine.Qty += groupedCodes[i].Count();
          _sqlDb.SaleLines_Update(saleLine.Id, saleLine.Qty, saleLine.DiscPct);
          groupedCodes.RemoveAt(i);
        }
      }
    }

    private async Task GetFromChannelAdvisor(List<IGrouping<string, string>> groupedCodes)
    {
      Dictionary<string, int> uniqueCodes = groupedCodes
        .ToDictionary(g => g.Key, g => g.Count(), StringComparer.InvariantCultureIgnoreCase);

      List<JObject> jObjects = await _ca.GetProductsByCodeAsync(groupedCodes.Select(g => g.Key).ToList());

      foreach (var item in jObjects)
      {
        int productId = 0;
        string sku = item[CAStrings.sku].ToString();
        string upc = item[CAStrings.upc].ToString();
        float cost = String.IsNullOrEmpty(item[CAStrings.cost].ToString()) ? 0 :
          item[CAStrings.cost].ToObject<float>();
        float price = item[CAStrings.attributes]
          .FirstOrDefault(i => i[CAStrings.name]
            .ToString() == CAStrings.bcprice)[CAStrings.Value]
          .ToObject<float>();
        string allName = item[CAStrings.attributes]
          .FirstOrDefault(i => i[CAStrings.name]
            .ToString() == CAStrings.allName)[CAStrings.Value]
          .ToString();

        ProductModel product = _sqlDb.Products_GetByCode(sku, upc);
        if (product == null)
          productId = _sqlDb.Products_Insert(sku, upc, cost, price, allName);
        else if (product.Cost != cost || product.Price != price || product.AllName != allName)
          _sqlDb.Products_Update(product.Id, cost, price, allName);
        else
          productId = product.Id;

        int qty1 = 0;
        int qty2 = 0;

        if (uniqueCodes.ContainsKey(sku))
        {
          qty1 = uniqueCodes[sku];
          groupedCodes.Remove(groupedCodes.FirstOrDefault(g => g.Key.ToLower() == sku.ToLower()));
        }

        if (uniqueCodes.ContainsKey(upc))
        {
          qty2 = uniqueCodes[upc];
          groupedCodes.Remove(groupedCodes.FirstOrDefault(g => g.Key == upc));
        }

        int qty = qty1 + qty2;

        _sqlDb.SaleLines_Insert(SaleId, productId, null, qty);
      }
    }
    #endregion

    public IActionResult OnPostRemoveProducts()
    {
      if (string.IsNullOrEmpty(Codes)) return RedirectToPage();

      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      List<ProductModel> products = _sqlDb.Products_GetBySaleId(SaleId);

      List<string> productCodes = Codes
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

    public IActionResult OnPostAddGiftCards()
    {
      List<string> msgs = new List<string>();
      List<string> giftCardCodes = GiftCardCodes
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

        int giftCardId = _sqlDb.GiftCards_Insert(code, GiftCardAmount);

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
      List<string> giftCardCodes = GiftCardCodes
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
          TempData["Message"] = $"{ code } was not found in the cart";
          continue;
        }

        int saleLineId = saleLines.FirstOrDefault(s => s.GiftCardId.Value == gc.Id).Id;
        _sqlDb.SaleLines_Delete(saleLineId);
        _sqlDb.GiftCards_Delete(gc.Id);
      }

      if (msgs.Count > 0) TempData["Message"] = string.Join(Environment.NewLine, msgs);

      return RedirectToPage();
    }
  }
}
