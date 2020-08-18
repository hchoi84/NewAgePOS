using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NewAgePOSLibrary.Models
{
  public class SaleTransactionModel
  {
    [Required]
    public float Amount { get; set; }

    [Required]
    [DisplayName("Payment Type")]
    public string PaymentType { get; set; }

    [Range(0, 200, ErrorMessage = "{2} characters max")]
    public string Message { get; set; }
  }
}
