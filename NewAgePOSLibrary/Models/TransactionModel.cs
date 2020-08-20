using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NewAgePOSLibrary.Models
{
  public class TransactionModel
  {
    [Required]
    [Range(0.00, 100.00)]
    public float Amount { get; set; }

    [Required]
    [DisplayName("Payment Type")]
    public string PaymentType { get; set; }

    public string Reason { get; set; }

    [Range(0, 200, ErrorMessage = "{2} characters max")]
    public string Message { get; set; }
  }
}
