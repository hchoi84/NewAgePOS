using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NewAgePOSModels.Models
{
  public class TransactionModel
  {
    public int SaleId { get; set; }
    public int? GiftCardId { get; set; }
    public float Amount { get; set; }
    public string Method { get; set; }
    public string Type { get; set; }
    public string Message { get; set; }
    public DateTime Created { get; set; }
  }
}
