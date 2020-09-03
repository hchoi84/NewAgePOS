using System;
using System.Collections.Generic;
using System.Text;

namespace NewAgePOSModels.Models
{
  public class RefundDataModel
  {
    public int SaleId { get; set; }
    public int TransactionId { get; set; }
    public float Amount { get; set; }
    public string Method { get; set; }
    public DateTime Created { get; set; }
    public int DiscPct { get; set; }
    public int RefundQty { get; set; }
    public float Price { get; set; }
    public float TaxPct { get; set; }
    public float LineTotal { get { return Price * RefundQty; } }
  }
}
