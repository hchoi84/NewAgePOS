using System;
using System.Collections.Generic;
using System.Text;

namespace NewAgePOSModels.Models
{
  public class TaxModel
  {
    public int Id { get; set; }
    public float TaxPct { get; set; }
    public bool IsDefault { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
  }
}
