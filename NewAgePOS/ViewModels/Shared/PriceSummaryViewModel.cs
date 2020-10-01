using NewAgePOS.Utilities;

namespace NewAgePOS.ViewModels.Shared
{
  public class PriceSummaryViewModel
  {
    public int SaleId { get; set; }
    public PathSourceEnum PathSource { get; set; }

    public int Quantity { get; set; }
    public float Subtotal { get; set; }
    public float Discount { get; set; }
    public float TradeInValue { get; set; }
    public float TaxPercent { get; set; }
    public float TaxAmount { get { return (Subtotal - Discount - TradeInValue) * (TaxPercent / 100f); } }
    public float Total { get { return Subtotal - Discount - TradeInValue + TaxAmount; } }
    
    public float Paid { get; set; }
    public float DueBalance { get { return Total - Paid; } }

    public float Change { get; set; }

    public float RefundedAmount { get; set; }
    public float RefundingAmount { get; set; }
  }
}
