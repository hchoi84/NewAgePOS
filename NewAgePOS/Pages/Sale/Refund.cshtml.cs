using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

    public List<SaleLineModel> SaleLines { get; set; }
    public List<RefundLineModel> RefundLines { get; set; } = new List<RefundLineModel>();
    public float TaxPct { get; set; }

    [BindProperty]
    [Display(Name = "SKUs or UPCs")]
    public string Codes { get; set; }

    [BindProperty]
    public string Message { get; set; }

    public IActionResult OnGet()
    {
      bool isComplete = _sqlDb.Sales_GetStatus(SaleId);
      if (!isComplete)
      {
        TempData["Message"] = $"Sale Id { SaleId } is not complete. Refund is unavailable.";
        return RedirectToPage("Search");
      }

      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      TaxPct = _sqlDb.Taxes_GetBySaleId(SaleId);

      SaleLines.ForEach(s => RefundLines.AddRange(_sqlDb.RefundLines_GetBySaleLineId(s.Id)));

      return Page();
    }

    public IActionResult OnPostAdd()
    {
      List<string> productCodes = Codes.Trim().Replace(" ", string.Empty).Split(Environment.NewLine).ToList();
      List<IGrouping<string, string>> groupedCodes = productCodes.GroupBy(p => p).ToList();

      SaleLines = SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);

      foreach (var groupedCode in groupedCodes)
      {
        SaleLineModel saleLine = SaleLines.FirstOrDefault(sl => sl.Sku == groupedCode.Key || sl.Upc == groupedCode.Key);

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
        SaleLineModel saleLine = SaleLines.FirstOrDefault(sl => sl.Sku == groupedCode.Key || sl.Upc == groupedCode.Key);

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
