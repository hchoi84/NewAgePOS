using NewAgePOSModels.Models;
using System.Collections.Generic;

namespace NewAgePOS.ViewModels.ViewComponent
{
  public class MessagesVCVM
  {
    public int SaleId { get; set; }
    public string Message { get; set; }
    public string ReturnUrl { get; set; }
    public List<MessageModel> Messages { get; set; }
  }
}
