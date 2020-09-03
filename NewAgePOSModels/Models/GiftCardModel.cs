using System;
using System.Collections.Generic;
using System.Text;

namespace NewAgePOSModels.Models
{
  public class GiftCardModel
  {
    public int Id { get; set; }
    public string Code { get; set; }
    public float Amount { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
  }
}
