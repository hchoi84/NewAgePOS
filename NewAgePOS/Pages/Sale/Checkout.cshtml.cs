using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOS.ViewModels.Sale;
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

    private ReadOnlyCollection<string> TransactionOrderPref { get; }
      = new ReadOnlyCollection<string>(new string[] { "GiftCard", "Cash", "Give" });

    [BindProperty(SupportsGet = true)]
    public int SaleId { get; set; }

    [BindProperty]
    public CheckoutViewModel Checkout { get; set; }

    public CustomerModel Customer { get; set; }
    public List<TransactionModel> Transactions { get; set; }
    public List<GiftCardModel> GiftCards { get; set; }
    public bool IsZeroBalance { get; set; }

    public IActionResult OnGet()
    {
      if (_sqlDb.Sales_GetById(SaleId).IsComplete)
      {
        TempData["Message"] = $"Cannot access Checkout because Sale Id { SaleId } was completed.";
        return RedirectToPage("Search");
      }

      Checkout = new CheckoutViewModel();
      Customer = _sqlDb.Customers_GetBySaleId(SaleId);
      Transactions = _sqlDb.Transactions_GetBySaleId(SaleId);
      GiftCards = Transactions.Where(t => t.GiftCardId.HasValue)
        .Select(t => _sqlDb.GiftCards_GetById(t.GiftCardId.Value))
        .ToList();

      float dueBalance = CalculateAndStoreRoundedValue();

      Transactions = Transactions
        .OrderBy(t => TransactionOrderPref.IndexOf(t.Method))
        .ToList();
      IsZeroBalance = dueBalance <= 0;
      Checkout.PaymentMethod = "Cash";

      return Page();
    }

    private float CalculateAndStoreRoundedValue()
    {
      float totalDue = CalculateDueBalance();

      float factor = 0.05f;
      float roundedTotalDue = (float)Math.Round(totalDue / factor, MidpointRounding.AwayFromZero) * (float)factor;
      float difference = totalDue - roundedTotalDue;
      if (difference == 0) return totalDue;

      TransactionModel giveTransaction = Transactions.FirstOrDefault(t => t.Method == "Give");
      if (giveTransaction == null)
      {
        _sqlDb.Transactions_Insert(SaleId, null, difference, "Give", "Checkout", Checkout.Message);
        Transactions = _sqlDb.Transactions_GetBySaleId(SaleId);
      }
      else if (giveTransaction.Amount != difference)
      {
        giveTransaction.Amount += difference;
        _sqlDb.Transactions_UpdateAmount(giveTransaction.Id, giveTransaction.Amount);
        Transactions = _sqlDb.Transactions_GetBySaleId(SaleId);
      }

      return totalDue;
    }

    private float CalculateDueBalance()
    {
      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      float purchaseAmount = saleLines.Where(sl =>
          sl.ProductId.HasValue || sl.GiftCardId.HasValue)
        .Sum(sl => (sl.Price - (sl.LineDiscountTotal / sl.Qty)) * sl.Qty);
      float tradeInAmount = saleLines.Where(sl => 
          !sl.ProductId.HasValue && !sl.GiftCardId.HasValue)
        .Sum(sl => sl.Price);

      float subtotalDue = purchaseAmount - tradeInAmount;
      float paid = Transactions.Sum(t => t.Amount);
      float taxPct = _sqlDb.Taxes_GetBySaleId(SaleId).TaxPct;
      float taxDue = subtotalDue * (taxPct / 100f);
      float totalDue = subtotalDue + taxDue - paid;
      return totalDue;
    }

    public IActionResult OnPostApplyPayments()
    {
      if (!ModelState.IsValid) return Page();

      if (string.IsNullOrEmpty(Checkout.PaymentMethod))
      {
        TempData["Message"] = "Please choose payment method";
        return RedirectToPage();
      }

      Transactions = _sqlDb.Transactions_GetBySaleId(SaleId);
      float dueBalance = CalculateDueBalance();

      List<string> msgs = new List<string>();

      if (!string.IsNullOrEmpty(Checkout.GiftCardCodes))
        dueBalance = ProcessGiftCards(dueBalance, msgs);
      else if (Checkout.PaymentMethod == "Cash" && Checkout.Amount > 0)
        dueBalance = ProcessCash(dueBalance);

      if (Checkout.GiveAmount != 0)
        ProcessGive(dueBalance);

      TempData["Message"] = string.Join(Environment.NewLine, msgs);

      return RedirectToPage(new { SaleId });
    }

    private float ProcessGiftCards(float dueBalance, List<string> msgs)
    {
      List<string> giftCardCodes = Checkout.GiftCardCodes.Trim().Split(Environment.NewLine).Distinct().ToList();
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

        _sqlDb.Transactions_Insert(SaleId, giftCard.Id, payingAmt, "GiftCard", "Checkout", Checkout.Message);
        _sqlDb.GiftCards_Update(giftCard.Id, giftCard.Amount - payingAmt);

        dueBalance -= payingAmt;
      }

      return dueBalance;
    }

    private float ProcessCash(float dueBalance)
    {
      TransactionModel cashTransaction = Transactions.Where(t => t.Method == "Cash" && t.Type == "Checkout").FirstOrDefault();

      if (cashTransaction != null)
      {
        cashTransaction.Amount += Checkout.Amount;
        _sqlDb.Transactions_UpdateAmount(cashTransaction.Id, cashTransaction.Amount);
      }
      else
      {
        _sqlDb.Transactions_Insert(SaleId, null, Checkout.Amount, Checkout.PaymentMethod, "Checkout", Checkout.Message);
      }

      return dueBalance -= Checkout.Amount;
    }

    private float ProcessGive(float dueBalance)
    {
      TransactionModel giveTransaction = Transactions.Where(t => t.Method == "Give" && t.Type == "Checkout").FirstOrDefault();

      if (giveTransaction != null)
      {
        giveTransaction.Amount += Checkout.GiveAmount;
        _sqlDb.Transactions_UpdateAmount(giveTransaction.Id, giveTransaction.Amount);
      }
      else
      {
        _sqlDb.Transactions_Insert(SaleId, null, Checkout.GiveAmount, "Give", "Checkout", Checkout.Message);
      }

      dueBalance -= Checkout.GiveAmount;
      return dueBalance;
    }

    public async Task<IActionResult> OnPostCompleteSaleAsync()
    {
      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      JObject result = await RemoveProductsFromSkuVault(saleLines);
      GenerateErrorMessage(result);

      _sqlDb.Sales_MarkComplete(SaleId);

      return RedirectToPage("Receipt", new { SaleId });
    }

    private async Task<JObject> RemoveProductsFromSkuVault(List<SaleLineModel> saleLines)
    {
      List<AddRemoveItemBulkModel> itemsToRemove = new List<AddRemoveItemBulkModel>();
      List<ProductModel> products = _sqlDb.Products_GetBySaleId(SaleId);

      foreach (SaleLineModel saleLine in saleLines)
      {
        if (!saleLine.ProductId.HasValue) continue;
        string sku = products.FirstOrDefault(p => p.Id == saleLine.ProductId.Value).Sku;

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
