using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NewAgePOSModels.Models
{
  public class SalePriceSummaryModel
  {
    public float Subtotal { get; set; }
    public float Discount { get; set; }
    public float TaxPct { get; set; }
    public float Tax { get; set; }
    public float Total { get; set; }
    public CultureInfo Dollar { get; } = new CultureInfo("en-US");
  }
}
