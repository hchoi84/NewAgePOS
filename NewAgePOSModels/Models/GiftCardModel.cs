using NewAgePOSModels.Utilities;
using System;

namespace NewAgePOSModels.Models
{
  public class GiftCardModel
  {
    private DateTime _created;

    public int Id { get; set; }
    public string Code { get; set; }
    public float Amount { get; set; }
    public DateTime Created {
      get { return _created.UTCtoPST(); }
      set { _created = value; }
    }
    public DateTime Updated { get; set; }
  }
}
