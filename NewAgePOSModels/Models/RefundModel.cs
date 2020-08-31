using System.ComponentModel.DataAnnotations;

namespace NewAgePOSModels.Models
{
  public class RefundQtyModel
  {
    public int SaleLineId { get; set; }

    [Display(Name = "Refund Qty")]
    [Range(0, int.MaxValue)]
    public int RefundQty { get; set; }
  }
}
