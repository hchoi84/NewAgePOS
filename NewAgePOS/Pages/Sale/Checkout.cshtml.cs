using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;
using Newtonsoft.Json.Linq;
using SkuVaultLibrary;

namespace NewAgePOS.Pages.Sale
{
  public class CheckoutModel : PageModel
  {
    private readonly ISQLData _sqlDb;
    private readonly ISkuVault _skuVault;

    public CheckoutModel(ISQLData sqlDb, ISkuVault skuVault)
    {
      _sqlDb = sqlDb;
      _skuVault = skuVault;
    }

    [BindProperty]
    public List<SelectListItem> PaymentMethods { get; } = new List<SelectListItem>
    {
      new SelectListItem { Value = "Cash", Text = "Cash"},
    };
    
    [BindProperty]
    [Required]
    [Range(0.00, float.MaxValue)]
    public float Amount { get; set; }

    [BindProperty(SupportsGet = true)]
    public int SaleId { get; set; }

    [BindProperty]
    [StringLength(200, ErrorMessage = "{1} Max characters")]
    public string Message { get; set; }

    [BindProperty]
    [Display(Name = "Gift Cards")]
    public string GiftCardCodes { get; set; }

    [BindProperty]
    public string PaymentMethod { get; set; }

    public List<SaleLineModel> SaleLines { get; set; } = new List<SaleLineModel>();
    public List<ProductModel> Products { get; set; } = new List<ProductModel>();
    public List<GiftCardModel> GiftCards { get; set; } = new List<GiftCardModel>();
    public List<TransactionModel> Transactions { get; set; } = new List<TransactionModel>();
    public CustomerModel Customer { get; set; }
    public float TaxPct { get; set; }

    public IActionResult OnGet()
    {
      bool isComplete = _sqlDb.Sales_GetById(SaleId).IsComplete;
      if (isComplete)
      {
        TempData["Message"] = $"Cannot access Checkout because Sale Id { SaleId } was completed.";
        return RedirectToPage("Search");
      }

      PaymentMethod = "Cash";

      _sqlDb.SaleLines_GetBySaleId(SaleId)
        .OrderByDescending(s => s.ProductId)
        .ToList()
        .ForEach(s =>
        {
          SaleLines.Add(s);

          if (s.ProductId.HasValue)
            Products.Add(_sqlDb.Products_GetById(s.ProductId.Value));
          if (s.GiftCardId.HasValue)
            GiftCards.Add(_sqlDb.GiftCards_GetById(s.GiftCardId.Value));
        });
      _sqlDb.Transactions_GetBySaleId(SaleId)
        .ForEach(t => 
        {
          Transactions.Add(t);

          if (t.GiftCardId.HasValue)
            GiftCards.Add(_sqlDb.GiftCards_GetById(t.GiftCardId.Value));
        });
      TaxPct = _sqlDb.Taxes_GetBySaleId(SaleId);
      Customer = _sqlDb.Customers_GetBySaleId(SaleId);

      return Page();
    }

    public async Task<IActionResult> OnPost()
    {
      if (string.IsNullOrEmpty(PaymentMethod))
      {
        TempData["Message"] = "Please choose payment method";
        return RedirectToPage();
      }

      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      Transactions = _sqlDb.Transactions_GetBySaleId(SaleId);
      TaxPct = _sqlDb.Taxes_GetBySaleId(SaleId);

      float saleTotal = SaleLines.Sum(s => (s.LineTotal - s.Discount) * (1 + TaxPct / 100f));
      float amountPaid = Transactions.Sum(t => t.Amount);
      float dueBalance = saleTotal - amountPaid;
      List<string> msgs = new List<string>();

      // Accept Cash
      if (PaymentMethod == "Cash" && Amount > 0)
      {
        _sqlDb.Transactions_Insert(SaleId, null, Amount, PaymentMethod, "Checkout", Message);
        dueBalance -= Amount;
      }

      // Accept Gift Card
      if (!string.IsNullOrEmpty(GiftCardCodes))
      {
        List<string> giftCardCodes = GiftCardCodes.Trim().Split(Environment.NewLine).Distinct().ToList();
        foreach (var code in giftCardCodes)
        {
          GiftCardModel giftCard = _sqlDb.GiftCards_GetByCode(code);

          if (giftCard == null)
          {
            msgs.Add($"{ code } was not found");
            continue;
          }

          if (giftCard.Amount <= 0) 
          { 
            msgs.Add($"{ code } has no balance");
            continue;
          }

          float payingAmt = giftCard.Amount < dueBalance ? giftCard.Amount : dueBalance;

          _sqlDb.Transactions_Insert(SaleId, giftCard.Id, payingAmt, "GiftCard", "Checkout", Message);
          _sqlDb.GiftCards_Update(giftCard.Id, giftCard.Amount - payingAmt);

          dueBalance -= payingAmt;
        }
      }

      if (dueBalance > 0)
      {
        TempData["Message"] = string.Join(Environment.NewLine, msgs);
        return RedirectToPage();
      }

      // If total due has been received, process:
      // 1. Removal of products
      // 2. Mark sales as complete
      List<AddRemoveItemBulkModel> itemsToRemove = 
        SaleLines.Where(s => s.ProductId != null)
        .ToList()
        .Select(s =>
        new AddRemoveItemBulkModel
        {
          Code = _sqlDb.Products_GetById(s.ProductId.Value).Sku,
          LocationCode = "STORE",
          Quantity = s.Qty,
          Reason = "Store Sale"
        })
        .ToList();

      JObject result = await _skuVault.RemoveItemBulkAsync(itemsToRemove);
      List<string> errorMsgs = new List<string>();

      if (result["Errors"].ToObject<JArray>().Any())
      {
        foreach (var e in (JArray)result["Errors"])
        {
          errorMsgs.Add($"{ e["Sku"] }: { e["ErrorMessages"][0] } { e["LocationCode"] }");
        }
      }

      TempData["Message"] = string.Join(Environment.NewLine, errorMsgs);

      _sqlDb.Sales_MarkComplete(SaleId);

      return RedirectToPage("Receipt", new { SaleId });
    }
  }
}
