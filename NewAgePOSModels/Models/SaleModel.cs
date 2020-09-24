using NewAgePOS.Utilities;
using NewAgePOSModels.Securities;
using System;

namespace NewAgePOSModels.Models
{
  public class SaleModel
  {
    private DateTime _created;

    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int TaxId { get; set; }
    public bool IsComplete { get; set; }
    public DateTime Created {
      get { return _created.USTtoPST(); }
      set { _created = value; }
    }
    public DateTime Updated { get; set; }
  }
}
