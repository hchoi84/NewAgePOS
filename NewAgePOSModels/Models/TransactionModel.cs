using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NewAgePOSModels.Models
{
  public class TransactionModel
  {
    public int SaleId { get; set; }

    [Required]
    [Range(0.00, 100.00)]
    public float Amount { get; set; }

    [Required]
    public string Method { get; set; }

    public string Type { get; set; }

    [Range(0, 200, ErrorMessage = "{2} characters max")]
    public string Message { get; set; }

    public DateTime Created { get; set; }
  }
}
