using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NewAgePOSModels.Models
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

    [DisplayName("Disc %")]
    [Range(0, 100, ErrorMessage = "{1} and {2}")]
    public float DiscPct { get; set; }

    [DisplayName("Line Total")]
    public float LineTotal { get { return Price * Qty; } }
  }
}
