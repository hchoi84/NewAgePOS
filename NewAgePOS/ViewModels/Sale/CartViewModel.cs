using System.ComponentModel.DataAnnotations;

namespace NewAgePOS.ViewModels.Sale
{
  public class CartViewModel
  {
    [Display(Name = "SKUs or UPCs")]
    public string Codes { get; set; }

    public string GiftCardCodes { get; set; }

    public float GiftCardAmount { get; set; }

    [Display(Name = "Value")]
    public float TradeInValue { get; set; }

    [Display(Name = "Confirm Value")]
    [Compare(nameof(TradeInValue))]
    public float ConfirmTradeInValue { get; set; }

    [Display(Name = "Quantity")]
    public int TradeInQty { get; set; }
  }
}
