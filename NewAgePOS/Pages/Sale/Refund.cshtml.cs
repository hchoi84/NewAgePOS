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
    [Display(Name = "Items to Refund SKUs or UPCs")]
    public string Codes { get; set; }

    [BindProperty]
    public string GiftCardCode { get; set; }

    public List<SelectListItem> RefundMethods { get; } = new List<SelectListItem>
    {
      new SelectListItem { Text = "Cash", Value = MethodEnum.Cash.ToString()},
      new SelectListItem { Text = "Gift Card", Value = MethodEnum.GiftCard.ToString() }
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

    public IActionResult OnPostApplyCashRefund(int saleId, float amount)
    {
      if (amount <= 0)
      {
        TempData["Message"] = "Nothing to refund";
        return RedirectToPage();
      }

      int transactionId = _sqlDb.Transactions_Insert(saleId, null, amount, MethodEnum.Cash, TypeEnum.Refund);

      IEnumerable<RefundLineModel> refunding = _sqlDb.RefundLines_GetBySaleId(saleId)
        .Where(r => !r.TransactionId.HasValue);

      foreach (var item in refunding)
      {
        _sqlDb.RefundLines_MarkComplete(item.Id, transactionId);
      }

      return RedirectToPage("Receipt", new { Id = transactionId, IdType = TypeEnum.Refund.ToString() });
    }

    public IActionResult OnPostApplyGiftCardRefund(int saleId, float amount, string giftCardCode)
    {
      if (amount <= 0)
      {
        TempData["Message"] = "Nothing to refund";
        return RedirectToPage();
      }

      if (string.IsNullOrEmpty(giftCardCode))
      {
        TempData["Message"] = "Gift Card code field is required";
        return RedirectToPage();
      }

      GiftCardModel gc = _sqlDb.GiftCards_GetByCode(giftCardCode);
      int gcId = 0;
      if (gc != null)
      {
        gcId = gc.Id;
        gc.Amount += amount;
        _sqlDb.GiftCards_Update(gc.Id, gc.Amount);
      }
      else
      {
        gcId = _sqlDb.GiftCards_Insert(giftCardCode, amount);
      }

      int transactionId = _sqlDb.Transactions_Insert(saleId, gcId, amount, MethodEnum.GiftCard, TypeEnum.Refund);

      IEnumerable<RefundLineModel> refunding = _sqlDb.RefundLines_GetBySaleId(saleId)
        .Where(r => !r.TransactionId.HasValue);

      foreach (var item in refunding)
      {
        _sqlDb.RefundLines_MarkComplete(item.Id, transactionId);
      }

      return RedirectToPage("Receipt", new { Id = transactionId, IdType = TypeEnum.Refund.ToString() });
    }

    public IActionResult OnPostAdd()
    {
      List<string> productCodes = Codes.Trim()
        .Split(Environment.NewLine)
        .Select(c => c.Trim())
        .ToList();
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
          return RedirectToPage();
        }

        SaleLineModel saleLine = saleLines.FirstOrDefault(sl => sl.ProductId == product.Id);
        int refundedQty = refundLines.Where(rl => rl.SaleLineId == saleLine.Id).Sum(rl => rl.Qty);

        if (groupedCode.Count() > saleLine.Qty - refundedQty)
        {
          TempData["Message"] = $"({ groupedCode.Key } - { groupedCode.Count() }): Can not refund more than purchased quantity";
          return RedirectToPage();
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
      List<string> productCodes = Codes.Trim()
        .Split(Environment.NewLine)
        .Select(c => c.Trim())
        .ToList();
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
          return RedirectToPage();
        }

        SaleLineModel saleLine = saleLines.FirstOrDefault(sl => sl.ProductId == product.Id);
        int saleLineId = saleLine.Id;

        RefundLineModel refundingLine = refundLines.FirstOrDefault(rl => 
          rl.SaleLineId == saleLineId && !rl.TransactionId.HasValue);
        int refundingQty = refundingLine != null ? refundingLine.Qty : 0;

        if (refundingQty == 0)
        {
          TempData["Message"] = $"{ groupedCode.Key }-{ groupedCode.Count() }: There are no pending refund quantity to remove";
          return RedirectToPage();
        }
        else if (refundingQty < groupedCode.Count())
        {
          TempData["Message"] = $"{ groupedCode.Key }-{ groupedCode.Count() }: Can not remove more than pending refund quantity of { refundingQty }";
          return RedirectToPage();
        }

        refundingLine.Qty -= groupedCode.Count();

        if (refundingLine.Qty == 0)
        {
          _sqlDb.RefundLines_Delete(refundingLine.Id);
        }
        else
        {
          _sqlDb.RefundLines_Update(refundingLine.Id, refundingLine.Qty);
        }
      }

      return RedirectToPage(new { SaleId });
    }
  }
}
