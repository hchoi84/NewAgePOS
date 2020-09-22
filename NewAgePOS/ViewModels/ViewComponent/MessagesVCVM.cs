using NewAgePOSModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewAgePOS.ViewModels.ViewComponent
{
  public class MessagesVCVM
  {
    public int SaleId { get; set; }
    public string Message { get; set; }
    public List<MessageModel> Messages { get; set; }
  }
}
