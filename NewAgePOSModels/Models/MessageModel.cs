using System;
using System.Collections.Generic;
using System.Text;

namespace NewAgePOSModels.Models
{
  public class MessageModel
  {
    public int Id { get; set; }
    public int SaleId { get; set; }
    public string Message { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
  }
}
