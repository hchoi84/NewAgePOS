using System.Collections.Generic;
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

    [BindProperty]
    public List<RefundQtyModel> Refunds { get; set; } = new List<RefundQtyModel>();

    [BindProperty]
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
        int refundedQty = _sqlDb.RefundLines_GetRefundQtyBySaleLineId(s.Id);
        s.Qty -= refundedQty;
        Refunds.Add(new RefundQtyModel{ SaleLineId = s.Id });
      });

      return Page();
    }

    public IActionResult OnPost()
    {
      SaleLines = _sqlDb.SaleLines_GetBySaleId(SaleId);

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

      float subtotal = SaleLines.Sum(sl => sl.RefundLineTotal);
      float discount = SaleLines.Sum(sl => (sl.RefundQty * sl.DiscAmt) + (sl.RefundQty * (sl.Price * (sl.DiscPct / 100f))));
      float tax = (subtotal - discount) * (TaxPct / 100f);
      float total = subtotal - discount + tax;

      int transactionId = _sqlDb.Transactions_Insert(SaleId, total, "Cash", "Refund", Message);

      refundsToApply.ForEach(r => _sqlDb.RefundLines_Insert(r.Id, transactionId, r.RefundQty));

      return RedirectToPage("RefundReceipt", new { transactionId });
    }
  }
}
