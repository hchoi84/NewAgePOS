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

    private readonly string _sku = "Sku";
    private readonly string _upc = "UPC";
    private readonly string _cost = "Cost";
    private readonly string _bcprice = "BCPrice";
    private readonly string _attributes = "Attributes";
    private readonly string _name = "Name";
    private readonly string _allName = "All Name";
    private readonly string _Value = "Value";

    public List<SaleLineModel> SaleLines { get; set; }
    public List<ProductModel> Products { get; set; } = new List<ProductModel>();
    public List<GiftCardModel> GiftCards { get; set; } = new List<GiftCardModel>();
    public float TaxPct { get; set; }

    [BindProperty(SupportsGet = true)]
    public int SaleId { get; set; }

    [BindProperty]
    [Display(Name = "SKUs or UPCs")]
    public string Codes { get; set; }

    [BindProperty]
    public List<CartDiscModel> CartDiscs { get; set; } = new List<CartDiscModel>();

    [BindProperty]
    public string GiftCardCodes { get; set; }

    [BindProperty]
    public float GiftCardAmount { get; set; }

    public IActionResult OnGet()
    {
      bool isComplete = _sqlDb.Sales_GetById(SaleId).IsComplete;
      if (isComplete)
      {
        TempData["Message"] = $"Cannot access Cart because Sale Id { SaleId } was completed.";
        return RedirectToPage("Search");
      }

      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      TaxPct = _sqlDb.Taxes_GetBySaleId(SaleId);

      foreach (var saleLine in SaleLines)
      {
        if (saleLine.ProductId != null) Products.Add(_sqlDb.Products_GetById(saleLine.ProductId.Value));
        if (saleLine.GiftCardId != null) GiftCards.Add(_sqlDb.GiftCards_GetById(saleLine.GiftCardId.Value));

        CartDiscs.Add(new CartDiscModel()
        {
          SaleLineId = saleLine.Id,
          DiscPct = saleLine.DiscPct
        });
      }

      return Page();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
      if (string.IsNullOrEmpty(Codes)) return RedirectToPage();

      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId).Where(s => s.ProductId != null).ToList();
      SaleLines.ForEach(s => Products.Add(_sqlDb.Products_GetById(s.ProductId.Value)));

      List<string> productCodes = Codes.Trim().Replace(" ", string.Empty).Split(Environment.NewLine).ToList();
      List<IGrouping<string, string>> groupedCodes = productCodes.GroupBy(p => p).ToList();

      UpdateCart(groupedCodes);
      if (groupedCodes.Count > 0) await GetFromChannelAdvisor(groupedCodes);
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
        ProductModel product = new ProductModel();
        SaleLineModel saleLine = new SaleLineModel();

        if (groupedCodes[i].Key.Contains("_"))
          product = Products.FirstOrDefault(p => p.Sku == groupedCodes[i].Key);
        else
          product = Products.FirstOrDefault(p => p.Upc == groupedCodes[i].Key);

        if (product != null)
        {
          saleLine = SaleLines.FirstOrDefault(s => s.ProductId.Value == product.Id);
          saleLine.Qty += groupedCodes[i].Count();
          _sqlDb.SaleLines_Update(saleLine.Id, saleLine.Qty, saleLine.DiscPct);
          groupedCodes.RemoveAt(i);
        }
      }
    }

    private async Task GetFromChannelAdvisor(List<IGrouping<string, string>> groupedCodes)
    {
      List<ProductModel> products = new List<ProductModel>();
      Dictionary<string, int> uniqueCodes = uniqueCodes = groupedCodes
        .ToDictionary(g => g.Key, g => g.Count(), StringComparer.InvariantCultureIgnoreCase);

      List<JObject> jObjects = await _ca.GetProductsByCodeAsync(groupedCodes.Select(g => g.Key).ToList());

      foreach (var item in jObjects)
      {
        int productId = 0;
        string sku = item[_sku].ToString();
        string upc = item[_upc].ToString();
        float cost = String.IsNullOrEmpty(item[_cost].ToString()) ? 0 : item[_cost].ToObject<float>();
        float price = item[_attributes]
          .FirstOrDefault(i => i[_name].ToString() == _bcprice)[_Value]
          .ToObject<float>();
        string allName = item[_attributes]
          .FirstOrDefault(i => i[_name].ToString() == _allName)[_Value]
          .ToString();

        ProductModel productDb = _sqlDb.Products_GetByCode(sku, upc);
        if (productDb == null)
          productId = _sqlDb.Products_Insert(sku, upc, cost, price, allName);
        else if (productDb.Cost != cost || productDb.Price != price || productDb.AllName != allName)
          _sqlDb.Products_Update(productDb.Id, cost, price, allName);
        else
          productId = productDb.Id;

        int qty1 = 0;
        int qty2 = 0;

        if (uniqueCodes.ContainsKey(sku))
        {
          qty1 = uniqueCodes[sku];
          groupedCodes.Remove(groupedCodes.FirstOrDefault(g => g.Key == sku.ToLower() || g.Key == sku.ToUpper()));
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

    public IActionResult OnPostApplyDiscount()
    {
      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);

      foreach (CartDiscModel cartDisc in CartDiscs)
      {
        SaleLineModel saleLine = SaleLines.FirstOrDefault(sl => sl.Id == cartDisc.SaleLineId);

        if (cartDisc.DiscPct > 100)
        {
          ModelState.AddModelError(string.Empty, "Discount percent can't be greater than 100");
          return Page();
        }

        if (saleLine.DiscPct != cartDisc.DiscPct)
        {
          _sqlDb.SaleLines_Update(cartDisc.SaleLineId, saleLine.Qty, cartDisc.DiscPct);
        }
      }

      return RedirectToPage();
    }

    public IActionResult OnPostRemove()
    {
      if (string.IsNullOrEmpty(Codes)) return RedirectToPage();

      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);

      List<string> productCodes = Codes.Trim().Replace(" ", string.Empty).Split(Environment.NewLine).ToList();
      List<IGrouping<string, string>> groupedCodes = productCodes.GroupBy(p => p).ToList();

      for (int i = groupedCodes.Count - 1; i >= 0; i--)
      {
        ProductModel product = new ProductModel();
        SaleLineModel saleLine = new SaleLineModel();

        if (groupedCodes[i].Key.Contains("_"))
          product = Products.FirstOrDefault(p => p.Sku == groupedCodes[i].Key);
        else
          product = Products.FirstOrDefault(p => p.Upc == groupedCodes[i].Key);

        if (product != null)
        {
          saleLine = SaleLines.FirstOrDefault(s => s.ProductId.Value == product.Id);
          saleLine.Qty -= groupedCodes[i].Count();

          if (saleLine.Qty <= 0) 
          { 
            _sqlDb.SaleLines_Delete(SaleLines[i].Id);
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

    public IActionResult OnPostAddGiftCard()
    {
      List<string> msgs = new List<string>();
      List<string> giftCardCodes = GiftCardCodes.Trim().Replace(" ", string.Empty).Split(Environment.NewLine).Distinct().ToList();

      foreach (string code in giftCardCodes)
      {
        GiftCardModel gc = _sqlDb.GiftCards_GetByCode(code);

        if (gc != null)
        {
          msgs.Add($"{ code } already exists. Skipping");
          continue;
        }

        int giftCardId = _sqlDb.GiftCards_Insert(code, GiftCardAmount);

        _sqlDb.SaleLines_Insert(SaleId, null, giftCardId, 1);
      }

      if (msgs.Count > 0) TempData["Message"] = string.Join(Environment.NewLine, msgs);

      return RedirectToPage();
    }

    public IActionResult OnPostRemoveGiftCard()
    {
      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId).Where(s => s.GiftCardId.HasValue).ToList();
      List<string> msgs = new List<string>();
      List<string> giftCardCodes = GiftCardCodes.Trim().Replace(" ", string.Empty).Split(Environment.NewLine).Distinct().ToList();

      foreach (string code in giftCardCodes)
      {
        GiftCardModel gc = _sqlDb.GiftCards_GetByCode(code);

        if (gc == null)
        {
          TempData["Message"] = $"{ code } was not found";
          continue;
        }

        _sqlDb.SaleLines_Delete(SaleLines.FirstOrDefault(s => s.GiftCardId.Value == gc.Id).Id);
        _sqlDb.GiftCards_Delete(gc.Id);
      }

      if (msgs.Count > 0) TempData["Message"] = string.Join(Environment.NewLine, msgs);

      return RedirectToPage();
    }
  }
}
