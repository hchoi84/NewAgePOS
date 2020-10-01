using NewAgePOSModels.Utilities;
using System;

namespace NewAgePOSModels.Models
{
  public class RefundLineModel
  {
    private DateTime _created;

    public int Id { get; set; }
    public int SaleLineId { get; set; }
    public int? TransactionId { get; set; }
    public int Qty { get; set; }
    public DateTime Created {
      get { return _created.UTCtoPST(); }
      set { _created = value; }
    }
    public DateTime Updated { get; set; }
  }
}
