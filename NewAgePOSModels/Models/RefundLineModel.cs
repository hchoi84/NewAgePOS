using System;
using System.Collections.Generic;
using System.Text;

namespace NewAgePOSModels.Models
{
  public class RefundLineModel
  {
    public int Id { get; set; }
    public int SaleLineId { get; set; }
    public int TransactionId { get; set; }
    public int Qty { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
  }
}
