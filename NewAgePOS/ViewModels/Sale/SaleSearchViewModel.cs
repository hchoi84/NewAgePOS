using System;

namespace NewAgePOS.ViewModels.Sale
{
  public class SearchViewModel
  {
    public int SaleId { get; set; }
    public bool IsComplete { get; set; }
    public string FullName { get; set; }
    public string EmailAddress { get; set; }
    public string PhoneNumber { get; set; }
    public int MessageCount { get; set; }
    public DateTime Created { get; set; }
  }
}
