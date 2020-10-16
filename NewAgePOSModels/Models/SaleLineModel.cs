using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NewAgePOSModels.Models
{
  public class SaleLineModel
  {
    public int Id { get; set; }
    public int SaleId { get; set; }
    public int? ProductId { get; set; }
    public int? GiftCardId { get; set; }
    public float Cost { get; set; }
    public float Price { get; set; }
    public int Qty { get; set; }
    public float DiscPct { get; set; }
    public float Subtotal { get { return Price * Qty; } }
    public float Discount { get { return Price * (DiscPct / 100f) * Qty; } }
    public float Total { get { return Subtotal - Discount; } }
  }
}
