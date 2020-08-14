using System;
using System.Collections.Generic;
using System.Text;

namespace ChannelAdvisorLibrary.Models
{
  public class ProductModel
  {
    public string Sku { get; set; }
    public string Upc { get; set; }
    public string AllName { get; set; }
    public float Cost { get; set; }
    public float Price { get; set; }
    public int Qty { get; set; }
    public float DiscAmt { get; set; }
    public float DiscPct { get; set; }
  }
}
