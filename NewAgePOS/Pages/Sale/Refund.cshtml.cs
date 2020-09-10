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
    public string Message { get; set; }

    [BindProperty]
    public string GiftCardCode { get; set; }

    public List<SelectListItem> RefundMethods { get; } = new List<SelectListItem>
    {
      new SelectListItem { Text = "Cash", Value = "Cash"},
      new SelectListItem { Text = "Gift Card", Value = "GiftCard" }
    };
    public List<SaleLineModel> SaleLines { get; set; }
    public List<ProductModel> Products { get; set; }
    public List<RefundLineModel> RefundLines { get; set; }
    public float TaxPct { get; set; }
    public float Subtotal { get; set; }

    // TODO: When refunding, calculate Give transaction. Subtract Give when refunding

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
      RefundLines = new List<RefundLineModel>();
      float refundableAmount = GetRefundableAmount();

      if (refundableAmount <= 0)
      {
        TempData["Message"] = "Nothing to refund";
        return Page();
      }

      int transactionId = 0;

      if (RefundMethod == "Cash")
      {
        transactionId = _sqlDb.Transactions_Insert(SaleId, null, refundableAmount, RefundMethod, "Refund", Message);
      }
      else if (RefundMethod == "GiftCard")
      {
        if (string.IsNullOrEmpty(GiftCardCode))
        {
          TempData["Message"] = "Gift Card Code can't be empty";
          return Page();
        }

        int giftCardId = _sqlDb.GiftCards_Insert(GiftCardCode, refundableAmount);
        transactionId = _sqlDb.Transactions_Insert(SaleId, giftCardId, refundableAmount, RefundMethod, "Refund", Message);
      }

      RefundLines.Where(r => r.TransactionId == 0).ToList().ForEach(r => _sqlDb.RefundLines_MarkComplete(r.Id, transactionId));

      return RedirectToPage("RefundReceipt", new { transactionId });
    }

    private float GetRefundableAmount()
    {
      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      TaxPct = _sqlDb.Taxes_GetBySaleId(SaleId).TaxPct;
      List<TransactionModel> transactions = _sqlDb.Transactions_GetBySaleId(SaleId);

      SaleLines.ForEach(s =>
      {
        List<RefundLineModel> refundLines = _sqlDb.RefundLines_GetBySaleLineId(s.Id);
        RefundLines.AddRange(refundLines);

        RefundLineModel refundingLine = refundLines.FirstOrDefault(r => r.TransactionId == 0);
        int refundingQty = refundingLine != null ? refundingLine.Qty : 0;

        Subtotal += (s.Price - (s.DiscPct / 100f * s.Price)) * refundingQty;
      });

      float total = Subtotal + (TaxPct / 100f * Subtotal);

      float checkoutAmt = transactions.Where(t => t.Type == "Checkout").Sum(t => t.Amount);
      float refundedAmt = transactions.Where(t => t.Type == "Refund").Sum(t => t.Amount);
      float giveAmt = transactions.Where(t => t.Method == "Give").Sum(t => t.Amount);

      float remainingRefundableAmount = checkoutAmt - refundedAmt - giveAmt;

      float refundableAmount = total > remainingRefundableAmount ? remainingRefundableAmount : total;
      
      return refundableAmount;
    }

    public IActionResult OnPostAdd()
    {
      Products = new List<ProductModel>();

      List<string> productCodes = Codes.Trim().Replace(" ", string.Empty).Split(Environment.NewLine).ToList();
      List<IGrouping<string, string>> groupedCodes = productCodes.GroupBy(p => p).ToList();

      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId).Where(s => s.ProductId != null).ToList();
      SaleLines.ForEach(s => Products.Add(_sqlDb.Products_GetById(s.ProductId.Value)));

      foreach (var groupedCode in groupedCodes)
      {
        ProductModel product = Products.FirstOrDefault(p => p.Sku == groupedCode.Key || p.Upc == groupedCode.Key);
        SaleLineModel saleLine = new SaleLineModel();
        if (product != null)  saleLine = SaleLines.FirstOrDefault(sl => sl.ProductId.Value == product.Id);

        if (saleLine == null)
        {
          TempData["Message"] = $"({ groupedCode.Key } - { groupedCode.Count() }): The product you're trying to refund does not exist in this sale";
          return RedirectToPage();
        }

        List<RefundLineModel> refundLines = _sqlDb.RefundLines_GetBySaleLineId(saleLine.Id);

        if (refundLines != null)
        {
          List<RefundLineModel> refundedLines = refundLines.Where(r => r.TransactionId != 0).ToList();
          RefundLineModel refundingLine = refundLines.FirstOrDefault(r => r.TransactionId == 0);
          int refundedQty = refundedLines != null ? refundedLines.Sum(r => r.Qty) : 0;
          int refundingQty = refundingLine != null ? refundingLine.Qty : 0;

          if (saleLine.Qty - refundedQty - refundingQty < groupedCode.Count())
          {
            TempData["Message"] = $"({ groupedCode.Key } - { groupedCode.Count() }): Can not refund more than purchased quantity";
            return RedirectToPage();
          }
        }

        _sqlDb.RefundLines_Insert(saleLine.Id, groupedCode.Count());
      }

      return RedirectToPage(new { SaleId });
    }

    public IActionResult OnPostRemove()
    {
      List<string> productCodes = Codes.Trim().Replace(" ", string.Empty).Split(Environment.NewLine).ToList();
      List<IGrouping<string, string>> groupedCodes = productCodes.GroupBy(p => p).ToList();

      SaleLines = SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);

      foreach (var groupedCode in groupedCodes)
      {
        ProductModel product = Products.FirstOrDefault(p => p.Sku == groupedCode.Key || p.Upc == groupedCode.Key);
        SaleLineModel saleLine = new SaleLineModel();
        if (product != null) saleLine = SaleLines.FirstOrDefault(sl => sl.ProductId.Value == product.Id);

        if (saleLine == null)
        {
          TempData["Message"] = $"The product you're trying to refund does not exist in this sale";
          return RedirectToPage();
        }

        List<RefundLineModel> refundLines = _sqlDb.RefundLines_GetBySaleLineId(saleLine.Id);

        if (refundLines == null)
        {
          TempData["Message"] = $"({ groupedCode.Key } - { groupedCode.Count() }): No pending refund quantity";
          return RedirectToPage();
        }

        RefundLineModel refundingLine = refundLines.FirstOrDefault(r => r.TransactionId == 0);
        int refundingQty = refundingLine != null ? refundingLine.Qty : 0;

        if (refundingQty < groupedCode.Count())
        {
          TempData["Message"] = $"({ groupedCode.Key } - { groupedCode.Count() }): Not enough pending refund quantity ({ refundingQty })";
          return RedirectToPage();
        }

        _sqlDb.RefundLines_SubtractQty(refundingLine.Id, groupedCode.Count());
      }

      return RedirectToPage(new { SaleId });
    }
  }
}
