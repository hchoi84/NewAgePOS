using System;
using System.Collections.Generic;
using System.Text;

namespace NewAgePOSModels.Models
{
  public class SaleModel
  {
    public bool IsComplete { get; set; }
    public string Message { get; set; }
    public DateTime Created { get; set; }
  }
}
