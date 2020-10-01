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

    public CustomerModel Customer { get; set; }
    public List<TransactionModel> Transactions { get; set; }
    public List<GiftCardModel> GiftCards { get; set; }

    public IActionResult OnGet()
    {
      if (_sqlDb.Sales_GetById(SaleId).IsComplete)
      {
        TempData["Message"] = $"Cannot access Checkout because Sale Id { SaleId } was completed.";
        return RedirectToPage("Search");
      }

      Customer = _sqlDb.Customers_GetBySaleId(SaleId);
      Transactions = _sqlDb.Transactions_GetBySaleId(SaleId);
      GiftCards = _sqlDb.GiftCards_GetBySaleId(SaleId);

      Transactions = Transactions
        .OrderBy(t => TransactionOrderPref.IndexOf(t.Method))
        .ToList();

      return Page();
    }

    public IActionResult OnPostProcessTransaction(float amount, MethodEnum method)
    {
      if (amount <= 0)
      {
        TempData["Message"] = $"{ method } amount must be greater than 0";
        return RedirectToPage();
      }

      TransactionModel transaction = _sqlDb.Transactions_GetBySaleId(SaleId)
        .FirstOrDefault(t => t.Type == TypeEnum.Checkout && t.Method == method);

      if (transaction != null)
      {
        transaction.Amount += amount;
        _sqlDb.Transactions_UpdateAmount(transaction.Id, transaction.Amount);
      }
      else
      {
        _sqlDb.Transactions_Insert(SaleId, null, amount, method, TypeEnum.Checkout);
      }

      return RedirectToPage();
    }

    public IActionResult OnPostProcessGiftCardTransaction(string giftCardCode, float dueBalance)
    {
      giftCardCode = giftCardCode.Trim();
      string errorMsg = "";

      if (string.IsNullOrEmpty(giftCardCode))
        errorMsg = "Gift Card Code is required";
      else if (giftCardCode.Length != 13)
        errorMsg = "Gift Card Code must be 13 digits";

      if (!string.IsNullOrEmpty(errorMsg))
      {
        TempData["Message"] = errorMsg;
        return RedirectToPage();
      }

      GiftCardModel gc = _sqlDb.GiftCards_GetByCode(giftCardCode);

      if (gc == null)
      {
        TempData["Message"] = $"Gift Card does not exist";
        return RedirectToPage();
      }

      float payingAmount = gc.Amount < dueBalance ? gc.Amount : dueBalance;

      gc.Amount -= payingAmount;
      _sqlDb.GiftCards_Update(gc.Id, gc.Amount);
      _sqlDb.Transactions_Insert(SaleId, gc.Id, payingAmount, MethodEnum.GiftCard, TypeEnum.Checkout);

      return RedirectToPage();
    }


    public async Task<IActionResult> OnPostCompleteSaleAsync(float change)
    {
      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      JObject result = await RemoveProductsFromSkuVault(saleLines);
      GenerateErrorMessage(result);

      if (change > 0)
        _sqlDb.Transactions_Insert(SaleId, null, change, MethodEnum.Change, TypeEnum.Checkout);

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
