using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NewAgePOSLibrary.Models
{
  public class SaleLineModel
  {
    public int Id { get; set; }
    public string Sku { get; set; }
    public string Upc { get; set; }
    public string AllName { get; set; }
    public float Cost { get; set; }
    public float Price { get; set; }

    [Range(0, 100, ErrorMessage = "{1} and {2}")]
    public int Qty { get; set; }

    [DisplayName("Disc $")]
    [Range(0, 100, ErrorMessage = "{1} and {2}")]
    public int DiscAmt { get; set; }

    [DisplayName("Disc %")]
    [Range(0, 100, ErrorMessage = "{1} and {2}")]
    public int DiscPct { get; set; }

    [DisplayName("Line Total")]
    public float LineTotal { get; set; }
    public bool IsUpdated { get; set; } = false;
  }
}
