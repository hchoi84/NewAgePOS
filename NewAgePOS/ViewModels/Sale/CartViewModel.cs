using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewAgePOS.ViewModels.Sale
{
  public class CartViewModel
  {
    public int SaleLineId { get; set; }
    public bool IsProduct { get; set; }
    public string Sku { get; set; }
    public string Upc { get; set; }
    public string AllName { get; set; }
    public float Cost { get; set; }
    public float Price { get; set; }
    public int Qty { get; set; }
  }
}
