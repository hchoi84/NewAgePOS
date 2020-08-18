using System;
using System.Collections.Generic;
using System.Text;

namespace NewAgePOSLibrary.Models
{
  public class SaleSearchModel
  {
    public int Id { get; set; }
    public string FullName { get; set; }
    public string EmailAddress { get; set; }
    public string PhoneNumber{ get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
  }
}
