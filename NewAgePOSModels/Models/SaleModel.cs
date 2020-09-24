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
      get {
        if (!Secrets.DBIsLocal)
        {
          TimeZoneInfo pst = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
          return TimeZoneInfo.ConvertTimeFromUtc(_created, pst);
        }
        else
        {
          return _created;
        }
      }
      set { _created = value; }
    }

    public DateTime Updated { get; set; }
  }
}
