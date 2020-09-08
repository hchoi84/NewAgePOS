using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewAgePOS.ViewModels.Sale
{
  public class CheckoutViewModel
  {
    public string PaymentMethod { get; set; }

    public List<SelectListItem> PaymentMethods { get; } = new List<SelectListItem>
    {
      new SelectListItem { Text = "Cash", Value = "Cash"},
    };

    [Range(0.00, float.MaxValue)]
    public float Amount { get; set; }

    [Display(Name = "Give Amount")]
    public float GiveAmount { get; set; }

    [Display(Name = "Confirm Give Amount")]
    [Compare(nameof(GiveAmount), ErrorMessage = "Amount doesn't match")]
    public float GiveAmountConfirmation { get; set; }

    [StringLength(200, ErrorMessage = "{1} Max characters")]
    public string Message { get; set; }

    [Display(Name = "Gift Cards")]
    public string GiftCardCodes { get; set; }

  }
}
