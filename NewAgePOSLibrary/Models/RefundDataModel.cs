using System;
using System.Collections.Generic;
using System.Text;

namespace NewAgePOSLibrary.Models
{
  public class RefundDataModel
  {
    public int SaleId { get; set; }
    public int SaleTransactionId { get; set; }
    public float Amount { get; set; }
    public string PaymentType { get; set; }
    public DateTime Created { get; set; }
    public int DiscAmt { get; set; }
    public int DiscPct { get; set; }
    public int RefundQty { get; set; }
    public float Price { get; set; }
    public float TaxPct { get; set; }
    public float LineTotal { get; set; }
  }
}
