using System;
using System.Collections.Generic;
using System.Text;

namespace NewAgePOSModels.Models
{
  public class ProductSearchModel
  {
    public string Sku { get; set; }
    public string Upc { get; set; }
    public string AllName { get; set; }
    public Dictionary<string, int> Location { get; set; } = new Dictionary<string, int>();
  }
}
