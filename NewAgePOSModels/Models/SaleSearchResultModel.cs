using System;

namespace NewAgePOSModels.Models
{
  public class SaleSearchResultModel
  {
    public int SaleId { get; set; }
    public bool IsComplete { get; set; }
    public string FullName { get; set; }
    public string EmailAddress { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime Created { get; set; }
  }
}
