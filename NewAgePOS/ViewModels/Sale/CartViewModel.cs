using System.ComponentModel.DataAnnotations;

namespace NewAgePOS.ViewModels.Sale
{
  public class CartViewModel
  {
    [Display(Name = "SKUs or UPCs")]
    public string Codes { get; set; }


    [Display(Name = "Gift Card Codes")]
    public string GiftCardCodes { get; set; }

    [Display(Name = "Amount")]
    public float GiftCardAmount { get; set; }

    [Display(Name = "Cofirm Amount")]
    [Compare(nameof(GiftCardAmount), ErrorMessage = "Gift Card amount does not match")]
    public float GiftCardAmountConfirm { get; set; }


    [Display(Name = "Trade In ID")]
    public int SaleLineId { get; set; }

    [Display(Name = "Value")]
    public float TradeInValue { get; set; }

    [Display(Name = "Confirm Value")]
    [Compare(nameof(TradeInValue), ErrorMessage = "Trade in values do not match")]
    public float ConfirmTradeInValue { get; set; }

    [Display(Name = "Quantity")]
    public int TradeInQty { get; set; }
  }
}
