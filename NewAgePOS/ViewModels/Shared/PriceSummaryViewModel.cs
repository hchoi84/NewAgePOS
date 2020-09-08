using System.Globalization;

namespace NewAgePOS.ViewModels.Shared
{
  public class PriceSummaryViewModel
  {
    public float Subtotal { get; set; }
    public float Discount { get; set; }
    public float Paid { get; set; }
    public float TaxPct { get; set; }
    public float TaxAmt { get { return (Subtotal - Discount) * (TaxPct / 100f); } }
    public float Total { get { return Subtotal - Discount + TaxAmt; } }
    public CultureInfo Dollar { get { return new CultureInfo("en-US"); } }
  }
}
