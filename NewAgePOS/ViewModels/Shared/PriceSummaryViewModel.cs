using System.Globalization;

namespace NewAgePOS.ViewModels.Shared
{
  public class PriceSummaryViewModel
  {
    public float Subtotal { get; set; }
    public float Discount { get; set; }
    public float TaxPct { get; set; }
    public float TaxAmt { get { return (Subtotal - Discount) * (TaxPct / 100f); } }
    public float Total { get { return Subtotal - Discount + TaxAmt; } }
    public float Paid { get { return PaidGiftCard + PaidCash + PaidGive; } }
    public float PaidGiftCard { get; set; }
    public float PaidCash { get; set; }
    public float PaidGive { get; set; }
    public float Change { get { return Paid - Total; } }
    public CultureInfo Dollar { get { return new CultureInfo("en-US"); } }
  }
}
