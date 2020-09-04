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

    public List<SaleLineModel> SaleLines { get; set; } = new List<SaleLineModel>();
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

      TaxPct = _sqlDb.Taxes_GetBySaleId(SaleId);

      _sqlDb.SaleLines_GetBySaleId(SaleId)
        .OrderByDescending(s => s.ProductId)
        .ToList()
        .ForEach(s =>
          {
            SaleLines.Add(s);

            if (s.ProductId != null)
              Products.Add(_sqlDb.Products_GetById(s.ProductId.Value));
            else
              GiftCards.Add(_sqlDb.GiftCards_GetById(s.GiftCardId.Value));

            CartDiscs.Add(new CartDiscModel()
            {
              SaleLineId = s.Id,
              DiscPct = s.DiscPct
            });
          });

      return Page();
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
          _sqlDb.SaleLines_Update(cartDisc.SaleLineId, saleLine.Qty, cartDisc.DiscPct);
      }

      return RedirectToPage();
    }

    #region Add Product
    public async Task<IActionResult> OnPostAddProductAsync()
    {
      if (string.IsNullOrEmpty(Codes)) return RedirectToPage();

      List<string> productCodes = Codes.Trim()
        .Replace(" ", string.Empty)
        .Split(Environment.NewLine)
        .ToList();
      List<IGrouping<string, string>> groupedCodes = productCodes.GroupBy(p => p).ToList();

      _sqlDb.SaleLines_GetBySaleId(SaleId)
        .ToList()
        .ForEach(s =>
          {
            if (s.ProductId != null)
            {
              SaleLines.Add(s);
              Products.Add(_sqlDb.Products_GetById(s.ProductId.Value));
            }
          });

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
          product = Products.FirstOrDefault(p => p.Sku.ToLower() == groupedCodes[i].Key.ToLower());
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

    public IActionResult OnPostRemoveProduct()
    {
      if (string.IsNullOrEmpty(Codes)) return RedirectToPage();

      _sqlDb.SaleLines_GetBySaleId(SaleId)
        .ToList()
        .ForEach(s =>
          {
            if (s.ProductId != null)
            {
              SaleLines.Add(s);
              Products.Add(_sqlDb.Products_GetById(s.ProductId.Value));
            }
          });

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
          product = Products.FirstOrDefault(p => p.Sku.ToLower() == groupedCodes[i].Key.ToLower());
        else
          product = Products.FirstOrDefault(p => p.Upc == groupedCodes[i].Key);

        if (product != null)
        {
          SaleLineModel saleLine = SaleLines.FirstOrDefault(s => s.ProductId.Value == product.Id);
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

    public IActionResult OnPostRemoveGiftCard()
    {
      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId).Where(s => s.GiftCardId.HasValue).ToList();

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
