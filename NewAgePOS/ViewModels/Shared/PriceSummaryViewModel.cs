using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace NewAgePOS.ViewModels.Shared
{
  public class PriceSummaryViewModel
  {
    public int Quantity { get; set; }
    public float Subtotal { get; set; }
    public float Discount { get; set; }
    public float TaxPct { get; set; }
    public float TaxAmt { get { return (Subtotal - Discount) * (TaxPct / 100f); } }
    public float Total { get { return Subtotal - Discount + TaxAmt; } }
    public float Paid { get { return PaidGiftCard + PaidCash + PaidGive; } }
    public float PaidGiftCard { get; set; }
    public float PaidCash { get; set; }
    public float PaidGive { get; set; }
    public float Remaining { get { return Total - Paid; } }
    public float Change { get { return Paid - Total; } }
    public float RefundedAmount { get; set; }
    public float RefundingAmount { get; set; }
    public CultureInfo Dollar { get { return new CultureInfo("en-US"); } }
  }
}
