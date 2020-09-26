using NewAgePOSModels.Utilities;
using System;

namespace NewAgePOSModels.Models
{
  public class MessageModel
  {
    private DateTime _created;

    public int Id { get; set; }
    public int SaleId { get; set; }
    public string Message { get; set; }
    public DateTime Created {
      get { return _created.UTCtoPST(); }
      set { _created = value; }
    }
    public DateTime Updated { get; set; }
  }
}
