using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;

namespace NewAgePOS.Pages.Sale
{
  public class RefundModel : PageModel
  {
    private readonly ISQLData _sqlDb;

    public RefundModel(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
    }

    [BindProperty(SupportsGet = true)]
    public int SaleId { get; set; }

    [BindProperty]
    public string RefundMethod { get; set; }

    [BindProperty]
    [Display(Name = "SKUs or UPCs")]
    public string Codes { get; set; }

    [BindProperty]
    [Display(Name = "Message (Optional)")]
    public string Message { get; set; }

    [BindProperty]
    public string GiftCardCode { get; set; }

    public List<SelectListItem> RefundMethods { get; } = new List<SelectListItem>
    {
      new SelectListItem { Text = "Cash", Value = "Cash"},
      new SelectListItem { Text = "Gift Card", Value = "GiftCard" }
    };

    public IActionResult OnGet()
    {
      bool isComplete = _sqlDb.Sales_GetById(SaleId).IsComplete;
      if (!isComplete)
      {
        TempData["Message"] = $"Sale Id { SaleId } is not complete. Refund is unavailable.";
        return RedirectToPage("Search");
      }

      return Page();
    }

    public IActionResult OnPost()
    {
      List<RefundLineModel> refundingLines = _sqlDb.RefundLines_GetBySaleId(SaleId).Where(rl => rl.TransactionId == 0).ToList();
      float refundingAmount = GetRefundingAmount(refundingLines);

      if (refundingAmount <= 0)
      {
        TempData["Message"] = "Nothing to refund";
        return Page();
      }

      int transactionId = 0;

      if (RefundMethod == "Cash")
      {
        transactionId = _sqlDb.Transactions_Insert(SaleId, null, refundingAmount, RefundMethod, "Refund", Message);
      }
      else if (RefundMethod == "GiftCard")
      {
        if (string.IsNullOrEmpty(GiftCardCode))
        {
          TempData["Message"] = "Gift Card Code can't be empty";
          return Page();
        }

        GiftCardModel giftCard = _sqlDb.GiftCards_GetByCode(GiftCardCode);
        int giftCardId = 0;

        if (giftCard != null)
        {
          giftCardId = giftCard.Id;
          giftCard.Amount += refundingAmount;
          _sqlDb.GiftCards_Update(giftCard.Id, giftCard.Amount);
        }
        else
        {
          giftCardId = _sqlDb.GiftCards_Insert(GiftCardCode, refundingAmount);
        }

        transactionId = _sqlDb.Transactions_Insert(SaleId, giftCardId, refundingAmount, RefundMethod, "Refund", Message);
      }

      refundingLines.ForEach(r => _sqlDb.RefundLines_MarkComplete(r.Id, transactionId));

      return RedirectToPage("Receipt", new { Id = transactionId, IdType = "Refund" });
    }

    private float GetRefundingAmount(List<RefundLineModel> refundingLines)
    {
      List<TransactionModel> transactions = _sqlDb.Transactions_GetBySaleId(SaleId);
      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);

      float refundableAmount = transactions.Where(t => t.Type == "Checkout").Sum(t => t.Amount)
        - transactions.Where(t => t.Type == "Refund").Sum(t => t.Amount);

      float refundingAmount = refundingLines.Sum(rl =>
      {
        SaleLineModel saleLine = saleLines.FirstOrDefault(sl => sl.Id == rl.SaleLineId);
        float priceAfterDiscount = saleLine.Price - saleLine.LineDiscountTotal / saleLine.Qty;
        return rl.Qty * priceAfterDiscount;
      });

      return refundableAmount > refundingAmount ? refundingAmount : refundableAmount;
    }

    public IActionResult OnPostAdd()
    {
      List<string> productCodes = Codes.Trim().Replace(" ", string.Empty).Split(Environment.NewLine).ToList();
      List<IGrouping<string, string>> groupedCodes = productCodes.GroupBy(p => p).ToList();

      List<ProductModel> products = _sqlDb.Products_GetBySaleId(SaleId);
      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      List<RefundLineModel> refundLines = _sqlDb.RefundLines_GetBySaleId(SaleId);

      foreach (var groupedCode in groupedCodes)
      {
        ProductModel product = products.FirstOrDefault(p => p.Sku == groupedCode.Key || p.Upc == groupedCode.Key);

        if (product == null)
        {
          TempData["Message"] = $"({ groupedCode.Key } - { groupedCode.Count() }): The product you're trying to refund does not exist in this sale";
          return Page();
        }

        SaleLineModel saleLine = saleLines.FirstOrDefault(sl => sl.ProductId == product.Id);
        int refundedQty = refundLines.Where(rl => rl.SaleLineId == saleLine.Id).Sum(rl => rl.Qty);

        if (groupedCode.Count() > saleLine.Qty - refundedQty)
        {
          TempData["Message"] = $"({ groupedCode.Key } - { groupedCode.Count() }): Can not refund more than purchased quantity";
          return Page();
        }

        RefundLineModel refundingLine = refundLines.FirstOrDefault(rl => rl.Id == saleLine.Id);

        if (refundingLine != null)
        {
          refundingLine.Qty += groupedCode.Count();
          _sqlDb.RefundLines_Update(refundingLine.Id, refundingLine.Qty);
        }
        else
        {
          _sqlDb.RefundLines_Insert(saleLine.Id, groupedCode.Count());
        }
      }

      return RedirectToPage(new { SaleId });
    }

    public IActionResult OnPostRemove()
    {
      List<string> productCodes = Codes.Trim().Replace(" ", string.Empty).Split(Environment.NewLine).ToList();
      List<IGrouping<string, string>> groupedCodes = productCodes.GroupBy(p => p).ToList();

      List<ProductModel> products = _sqlDb.Products_GetBySaleId(SaleId);
      List<SaleLineModel> saleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      List<RefundLineModel> refundLines = _sqlDb.RefundLines_GetBySaleId(SaleId);

      foreach (var groupedCode in groupedCodes)
      {
        ProductModel product = products.FirstOrDefault(p => p.Sku == groupedCode.Key || p.Upc == groupedCode.Key);

        if (product == null)
        {
          TempData["Message"] = $"({ groupedCode.Key } - { groupedCode.Count() }): The product does not exist in this sale";
          return Page();
        }

        int saleLineId = saleLines.FirstOrDefault(sl => sl.ProductId == product.Id).Id;
        RefundLineModel refundingLine = refundLines.FirstOrDefault(rl => rl.SaleLineId == saleLineId);
        refundingLine.Qty -= groupedCode.Count();

        if (refundingLine.Qty < 0)
        {
          TempData["Message"] = $"({ groupedCode.Key } - { groupedCode.Count() }): Can not remove more than pending refund quantity";
          return Page();
        }

        _sqlDb.RefundLines_Update(refundingLine.Id, refundingLine.Qty);
      }

      return RedirectToPage(new { SaleId });
    }
  }
}
