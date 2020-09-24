namespace NewAgePOS.ViewModels.Shared
{
  public class PriceSummaryViewModel
  {
    public int Quantity { get; set; }
    public float Subtotal { get; set; }
    public float DiscountAmount { get; set; }
    public float GiveAmount { get; set; }
    public float TotalDiscount { get { return DiscountAmount + GiveAmount; } }
    public float TradeInAmount { get; set; }
    public float TaxPct { get; set; }
    public float TaxAmt { get { return (Subtotal - TotalDiscount - TradeInAmount) * (TaxPct / 100f); } }
    public float Total { get { return Subtotal - TotalDiscount - TradeInAmount + TaxAmt; } }
    public float Paid { get { return PaidGiftCard + PaidCash; } }
    public float PaidGiftCard { get; set; }
    public float PaidCash { get; set; }
    public float Remaining { get { return Total - Paid; } }
    public float Change { get; set; }
    public float RefundedAmount { get; set; }
    public float RefundingAmount { get; set; }
  }
}
