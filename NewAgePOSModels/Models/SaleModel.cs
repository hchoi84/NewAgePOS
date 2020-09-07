using System;

namespace NewAgePOSModels.Models
{
  public class SaleModel
  {
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int TaxId { get; set; }
    public bool IsComplete { get; set; }
    public string Message { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
  }
}
