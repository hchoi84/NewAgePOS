using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
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

    public List<SaleLineModel> SaleLines { get; set; }
    public List<ProductModel> Products { get; set; }
    public List<GiftCardModel> GiftCards { get; set; }
    public List<TransactionModel> Transactions { get; set; }
    public CustomerModel Customer { get; set; }
    public float TaxPct { get; set; }

    public void Initialize()
    {
      Products = new List<ProductModel>();
      GiftCards = new List<GiftCardModel>();
      Transactions = new List<TransactionModel>();

      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId)
        .OrderByDescending(s => s.ProductId)
        .ToList();

      Transactions = _sqlDb.Transactions_GetBySaleId(SaleId);
      TaxPct = _sqlDb.Taxes_GetBySaleId(SaleId);
      Customer = _sqlDb.Customers_GetBySaleId(SaleId);
      PaymentMethod = "Cash";

      SaleLines.ForEach(s =>
      {
        if (s.ProductId.HasValue)
          Products.Add(_sqlDb.Products_GetById(s.ProductId.Value));
        if (s.GiftCardId.HasValue)
          GiftCards.Add(_sqlDb.GiftCards_GetById(s.GiftCardId.Value));
      });

      Transactions.ForEach(t =>
      {
        if (t.GiftCardId.HasValue)
          GiftCards.Add(_sqlDb.GiftCards_GetById(t.GiftCardId.Value));
      });
    }

    public IActionResult OnGet()
    {
      bool isComplete = _sqlDb.Sales_GetById(SaleId).IsComplete;
      if (isComplete)
      {
        TempData["Message"] = $"Cannot access Checkout because Sale Id { SaleId } was completed.";
        return RedirectToPage("Search");
      }

      Initialize();

      return Page();
    }

    public async Task<IActionResult> OnPost()
    {
      if (string.IsNullOrEmpty(PaymentMethod))
      {
        TempData["Message"] = "Please choose payment method";
        return RedirectToPage();
      }

      Initialize();

      float saleTotal = SaleLines.Sum(s => (s.LineTotal - s.Discount) * (1 + TaxPct / 100f));
      float amountPaid = Transactions.Sum(t => t.Amount);
      float dueBalance = saleTotal - amountPaid;
      List<string> msgs = new List<string>();

      ProcessCash(dueBalance);
      ProcessGiftCards(dueBalance, msgs);

      if (dueBalance > 0)
      {
        TempData["Message"] = string.Join(Environment.NewLine, msgs);
        return RedirectToPage();
      }

      JObject result = await RemoveProducts();
      GenerateErrorMessage(result);

      _sqlDb.Sales_MarkComplete(SaleId);

      return RedirectToPage("Receipt", new { SaleId });
    }

    private void ProcessCash(float dueBalance)
    {
      if (PaymentMethod == "Cash" && Amount > 0)
      {
        _sqlDb.Transactions_Insert(SaleId, null, Amount, PaymentMethod, "Checkout", Message);
        dueBalance -= Amount;
      }
    }

    private void ProcessGiftCards(float dueBalance, List<string> msgs)
    {
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
    }

    private async Task<JObject> RemoveProducts()
    {
      List<AddRemoveItemBulkModel> itemsToRemove = new List<AddRemoveItemBulkModel>();

      foreach (SaleLineModel saleLine in SaleLines)
      {
        if (!saleLine.ProductId.HasValue) continue;
        string sku = Products.FirstOrDefault(p => p.Id == saleLine.ProductId.Value).Sku;

        itemsToRemove.Add(new AddRemoveItemBulkModel
        {
          Code = sku,
          LocationCode = "STORE",
          Quantity = saleLine.Qty,
          Reason = "Store Sale"
        });
      }

      JObject result = await _skuVault.RemoveItemBulkAsync(itemsToRemove);
      return result;
    }

    private void GenerateErrorMessage(JObject result)
    {
      List<string> errorMsgs = new List<string>();

      if (result["Errors"].ToObject<JArray>().Any())
      {
        foreach (var e in (JArray)result["Errors"])
        {
          errorMsgs.Add($"{ e["Sku"] }: { e["ErrorMessages"][0] } { e["LocationCode"] }");
        }
      }

      TempData["Message"] = string.Join(Environment.NewLine, errorMsgs);
    }
  }
}
