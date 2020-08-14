using System;
using System.Collections.Generic;
using System.Text;

namespace NewAgePOSLibrary.Models
{
  public class SaleLineModel
  {
    public int Id { get; set; }
    public string Sku { get; set; }
    public string Upc { get; set; }
    public float Cost { get; set; }
    public float Price { get; set; }
    public int Qty { get; set; }
    public float DiscAmt { get; set; }
    public int DiscPct { get; set; }
    public float LineTotal { get; set; }
    public string AllName { get; set; }
  }
}
