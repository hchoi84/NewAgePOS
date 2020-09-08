using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOS.Utilities;
using NewAgePOS.ViewModels.Sale;
using NewAgePOS.ViewModels.Shared;
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
    private readonly IShare _share;

    public CheckoutModel(ISQLData sqlDb, ISkuVault skuVault, IShare share)
    {
      _sqlDb = sqlDb;
      _skuVault = skuVault;
      _share = share;
    }

    private ReadOnlyCollection<string> TransactionOrderPref { get; }
      = new ReadOnlyCollection<string>(new string[] { "GiftCard", "Cash", "Give" });

    [BindProperty(SupportsGet = true)]
    public int SaleId { get; set; }

    [BindProperty]
    public CheckoutViewModel Checkout { get; set; }

    public List<CartViewModel> Items { get; set; }
    public CustomerModel Customer { get; set; }
    public List<TransactionModel> Transactions { get; set; }
    public List<GiftCardModel> GiftCards { get; set; }
    public bool IsComplete { get; set; }
    public PriceSummaryViewModel PriceSummary { get; set; }

    public IActionResult OnGet()
    {
      if (_sqlDb.Sales_GetById(SaleId).IsComplete)
      {
        TempData["Message"] = $"Cannot access Checkout because Sale Id { SaleId } was completed.";
        return RedirectToPage("Search");
      }

      Customer = _sqlDb.Customers_GetBySaleId(SaleId);
      Transactions = _sqlDb.Transactions_GetBySaleId(SaleId)
        .OrderBy(t => t.Method.CompareTo(TransactionOrderPref))
        .ToList();
      GiftCards = Transactions.Where(t => t.GiftCardId.HasValue)
        .Select(t => _sqlDb.GiftCards_GetById(t.GiftCardId.Value))
        .ToList();
      Items = _share.GenerateCartViewModel(SaleId);
      PriceSummary = _share.GeneratePriceSummaryViewModel(SaleId);
      IsComplete = PriceSummary.Total - PriceSummary.Paid <= 0;
      Checkout.PaymentMethod = "Cash";

      return Page();
    }

    public IActionResult OnPostApplyPayments()
    {
      if (!ModelState.IsValid) return Page();

      if (string.IsNullOrEmpty(Checkout.PaymentMethod))
      {
        TempData["Message"] = "Please choose payment method";
        return RedirectToPage();
      }

      PriceSummary = _share.GeneratePriceSummaryViewModel(SaleId);
      float dueBalance = PriceSummary.Total - PriceSummary.Paid;

      List<string> msgs = new List<string>();

      if (!string.IsNullOrEmpty(Checkout.GiftCardCodes))
        dueBalance = ProcessGiftCards(dueBalance, msgs);
      else if (Checkout.PaymentMethod == "Cash" && Checkout.Amount > 0)
        dueBalance = ProcessCash(dueBalance);

      if (Checkout.GiveAmount > 0)
        ProcessGive(dueBalance);
      
      TempData["Message"] = string.Join(Environment.NewLine, msgs);

      return RedirectToPage();
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
      _sqlDb.Transactions_Insert(SaleId, null, Checkout.Amount, Checkout.PaymentMethod, "Checkout", Checkout.Message);

      dueBalance -= Checkout.Amount;

      return dueBalance;
    }

    private float ProcessGive(float dueBalance)
    {
      _sqlDb.Transactions_Insert(SaleId, null, Checkout.GiveAmount, "Give", "Checkout", Checkout.Message);
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
