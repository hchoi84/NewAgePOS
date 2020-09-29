using System.ComponentModel.DataAnnotations;

namespace NewAgePOS.ViewModels.Sale
{
  public class CartViewModel
  {
    [Display(Name = "SKUs or UPCs")]
    public string ProductCodes { get; set; }


    [Display(Name = "Gift Card Codes")]
    public string GiftCardCodes { get; set; }

    [Display(Name = "Amount")]
    public float GiftCardAmount { get; set; }

    [Display(Name = "Cofirm Amount")]
    public float GiftCardAmountConfirm { get; set; }


    [Display(Name = "Trade In ID")]
    public int SaleLineId { get; set; }

    [Display(Name = "Value")]
    public float TradeInValue { get; set; }

    [Display(Name = "Confirm Value")]
    public float ConfirmTradeInValue { get; set; }

    [Display(Name = "Quantity")]
    public int TradeInQty { get; set; }
  }
}
