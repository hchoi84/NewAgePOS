using NewAgePOSModels.Utilities;
using System;

namespace NewAgePOSModels.Models
{
  public class TransferRequestItemModel
  {
    private DateTime _created;
    private DateTime _updated;


    public int Id { get; set; }
    public int TransferRequestId { get; set; }
    public string Sku { get; set; }
    public int Qty { get; set; }
    public DateTime Created {
      get { return _created.UTCtoPST(); }
      set { _created = value; }
    }
    public DateTime Updated {
      get { return _updated.UTCtoPST(); }
      set { _updated = value; }
    }
  }
}
