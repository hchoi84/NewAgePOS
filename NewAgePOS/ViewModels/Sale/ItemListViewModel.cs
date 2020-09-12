using NewAgePOS.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace NewAgePOS.ViewModels.Sale
{
  public class ItemListViewModel
  {
    public int SaleLineId { get; set; }
    public bool IsProduct { get; set; }
    public bool IsGiftCard { get; set; }
    public string Sku { get; set; }
    public string Upc { get; set; }
    public string AllName { get; set; }
    public float Cost { get; set; }
    public float Price { get; set; }
    public int Qty { get; set; }
    public float DiscPct { get; set; }
    public CultureInfo Dollar { get { return new CultureInfo("en-US"); } }
  }
}
