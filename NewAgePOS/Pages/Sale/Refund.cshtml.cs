using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOSLibrary.Data;
using NewAgePOSLibrary.Models;

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

    [BindProperty(SupportsGet = true)]
    public List<SaleLineModel> SaleLines { get; set; }

    [BindProperty(SupportsGet = true)]
    public float TaxPct { get; set; }

    [BindProperty]
    public string Message { get; set; }

    public IActionResult OnGet()
    {
      bool isComplete = _sqlDb.Sales_GetStatus(SaleId);
      if (!isComplete)
      {
        TempData["Message"] = $"Sale Id { SaleId } is not complete. Refund is unavailable.";
        return RedirectToPage("Index");
      }

      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);
      TaxPct = _sqlDb.Taxes_GetBySaleId(SaleId);

      SaleLines.ForEach(s =>
      {
        s.Qty -= _sqlDb.RefundLines_GetRefundQtyBySaleLineId(s.Id);
        s.LineTotal = (s.Price - s.DiscAmt) * (1 - s.DiscPct / 100f) * s.Qty;
      });

      return Page();
    }

    public IActionResult OnPost()
    {
      // Validate
      foreach (var saleLine in SaleLines)
      {
        if (saleLine.RefundQty > saleLine.Qty)
        {
          ModelState.AddModelError(string.Empty, "Refund quantity cannot be greater than sold quantity");
          return Page();
        }

        if (saleLine.Qty - saleLine.RefundQty < 0)
        {
          ModelState.AddModelError(string.Empty, "Cannot refund more than sold quantity");
          return Page();
        }
      }

      // Update SaleLine
      List<SaleLineModel> refundsToApply = SaleLines.Where(s => s.RefundQty > 0).ToList();
      refundsToApply.ForEach(r => r.RefundLineTotal = (r.Price - r.DiscAmt) * (1 - r.DiscPct / 100f) * r.RefundQty);

      float subtotal = SaleLines.Sum(sl => sl.RefundLineTotal);
      float discount = SaleLines.Sum(sl => (sl.RefundQty * sl.DiscAmt) + (sl.RefundQty * (sl.Price * sl.DiscPct / 100))) * -1;
      float tax = (subtotal - discount) * (TaxPct / 100f);
      float total = subtotal - discount + tax;

      int saleTransactionId = _sqlDb.SaleTransaction_Insert(SaleId, total, "Cash", "Refund", Message);

      refundsToApply.ForEach(r => _sqlDb.RefundLines_Insert(r.Id, saleTransactionId, r.RefundQty));

      return RedirectToPage("RefundReceipt", new { saleTransactionId });
    }
  }
}
