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

    private ReadOnlyCollection<MethodEnum> TransactionOrderPref { get; }
      = new ReadOnlyCollection<MethodEnum>(new MethodEnum[] { MethodEnum.GiftCard, MethodEnum.Cash, MethodEnum.Give });

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

      Initialize();

      return Page();
    }

    private void Initialize()
    {
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
    }

    private float CalculateAndStoreRoundedValue()
    {
      float totalDue = (float)Math.Round(CalculateDueBalance(), 2);

      float factor = 0.05f;
      float roundedTotalDue = (int)(totalDue / factor) * factor;
      float difference = totalDue - roundedTotalDue;
      if (difference == 0) return totalDue;

      TransactionModel giveTransaction = Transactions.FirstOrDefault(t => t.Method == MethodEnum.Give);
      if (giveTransaction == null)
      {
        _sqlDb.Transactions_Insert(SaleId, null, difference, MethodEnum.Give, TypeEnum.Checkout);
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
      List<TransactionModel> transactions = new List<TransactionModel>();
      if (Transactions != null && Transactions.Any()) transactions = Transactions;
      else transactions = _sqlDb.Transactions_GetBySaleId(SaleId);

      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      float purchaseAmount = saleLines.Where(sl =>
          sl.ProductId.HasValue || sl.GiftCardId.HasValue)
        .Sum(sl => (sl.Price - (sl.LineDiscountTotal / sl.Qty)) * sl.Qty);
      float tradeInAmount = saleLines.Where(sl =>
          !sl.ProductId.HasValue && !sl.GiftCardId.HasValue)
        .Sum(sl => sl.Price);

      float subtotalDue = purchaseAmount - tradeInAmount;
      float paid = transactions == null ? 0 : transactions.Sum(t => t.Amount);
      float taxPct = _sqlDb.Taxes_GetBySaleId(SaleId).TaxPct;
      float taxDue = subtotalDue * (taxPct / 100f);
      float totalDue = subtotalDue + taxDue - paid;
      return totalDue;
    }


    public IActionResult OnPostApplyPayment()
    {
      if (string.IsNullOrEmpty(Checkout.PaymentMethod))
      {
        TempData["Message"] = "Please choose a Payment Method";
        return RedirectToPage();
      }

      float dueBalance = CalculateDueBalance();
      if (Checkout.Amount > dueBalance)
      {
        TempData["Message"] = $"Due balance is {dueBalance} and you are trying to collect {Checkout.Amount} which is greater than the due balance";
        return RedirectToPage();
      }

      if (Checkout.PaymentMethod == MethodEnum.Cash.ToString() && Checkout.Amount > 0)
        ProcessCash();

      return RedirectToPage(new { SaleId });
    }

    private void ProcessCash()
    {
      TransactionModel cashTransaction = _sqlDb.Transactions_GetBySaleId(SaleId)
        .Where(t => 
          t.Method == MethodEnum.Cash && 
          t.Type == TypeEnum.Checkout)
        .FirstOrDefault();

      if (cashTransaction != null)
      {
        cashTransaction.Amount += Checkout.Amount;
        _sqlDb.Transactions_UpdateAmount(cashTransaction.Id, cashTransaction.Amount);
      }
      else
      {
        _sqlDb.Transactions_Insert(SaleId, null, Checkout.Amount, MethodEnum.Cash, TypeEnum.Checkout);
      }
    }


    public IActionResult OnPostApplyGiftCards()
    {
      if (string.IsNullOrEmpty(Checkout.GiftCardCodes))
      {
        TempData["Message"] = "Must enter at least 1 gift card code";
        return RedirectToPage();
      }

      List<string> msgs = new List<string>();
      float dueBalance = CalculateDueBalance();
      List<string> giftCardCodes = Checkout.GiftCardCodes
        .Trim()
        .Split(Environment.NewLine)
        .Select(g => g.Trim())
        .Distinct()
        .ToList();

      foreach (var code in giftCardCodes)
      {
        GiftCardModel giftCard = _sqlDb.GiftCards_GetByCode(code);

        if (giftCard == null)
        {
          msgs.Add($"{ code } was not found");
          continue;
        }
        else if (giftCard.Amount <= 0)
        {
          msgs.Add($"{ code } has no balance");
          continue;
        }

        float payingAmt = giftCard.Amount < dueBalance ? giftCard.Amount : dueBalance;

        _sqlDb.Transactions_Insert(SaleId, giftCard.Id, payingAmt, MethodEnum.GiftCard, TypeEnum.Checkout);
        _sqlDb.GiftCards_Update(giftCard.Id, giftCard.Amount - payingAmt);
      }

      TempData["Message"] = string.Join(Environment.NewLine, msgs);
      return RedirectToPage();
    }

    public IActionResult OnPostApplyGiveAmount()
    {
      float dueBalance = CalculateDueBalance();
      List<TransactionModel> transactions = _sqlDb.Transactions_GetBySaleId(SaleId);

      TransactionModel giveTransaction = transactions.Where(t => 
          t.Method == MethodEnum.Give && 
          t.Type == TypeEnum.Checkout)
        .FirstOrDefault();

      if (giveTransaction != null)
      {
        giveTransaction.Amount += Checkout.GiveAmount;
        _sqlDb.Transactions_UpdateAmount(giveTransaction.Id, giveTransaction.Amount);
      }
      else
      {
        _sqlDb.Transactions_Insert(SaleId, null, Checkout.GiveAmount, MethodEnum.Give, TypeEnum.Checkout);
      }

      return RedirectToPage(new { SaleId });
    }


    public async Task<IActionResult> OnPostCompleteSaleAsync()
    {
      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      JObject result = await RemoveProductsFromSkuVault(saleLines);
      GenerateErrorMessage(result);

      _sqlDb.Sales_MarkComplete(SaleId);

      return RedirectToPage("Receipt", new { Id = SaleId, IdType = "Sale" });
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


    public IActionResult OnPostDeleteTransaction(int transactionId)
    {
      bool isSaleComplete = _sqlDb.Sales_GetById(SaleId).IsComplete;
      if (isSaleComplete)
      {
        TempData["Message"] = "Can not delete transaction because Sale has been completed";
        return RedirectToPage("Search");
      }

      TransactionModel transaction = _sqlDb.Transactions_GetById(transactionId);
      bool isGiftCardTransaction = transaction.GiftCardId.HasValue;
      if (isGiftCardTransaction)
      {
        GiftCardModel giftCard = _sqlDb.GiftCards_GetById(transaction.GiftCardId.Value);
        giftCard.Amount += transaction.Amount;
        _sqlDb.GiftCards_Update(giftCard.Id, giftCard.Amount);
      }

      _sqlDb.Transactions_DeleteById(transactionId);

      return RedirectToPage(new { SaleId });
    }
  }
}
