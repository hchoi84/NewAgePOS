using NewAgePOSModels.Utilities;
using System;

namespace NewAgePOSModels.Models
{
  public class TransferRequestModel
  {
    private StatusEnum _status;
    private DateTime _created;
    private DateTime _updated;

    public int Id { get; set; }
    public string Description { get; set; }
    public string CreatorName { get; set; }
    public StatusEnum Status {
      get { return _status; }
      set { _status = (StatusEnum)Enum.Parse(typeof(StatusEnum), value.ToString()); }
    }
    public DateTime Created {
      get { return _created.PSTtoUTC(); }
      set { _created = value; } 
    }
    public DateTime Updated {
      get { return _updated.PSTtoUTC(); }
      set { _updated = value; }
    }
  }
}
