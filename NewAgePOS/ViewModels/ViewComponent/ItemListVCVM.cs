using NewAgePOS.Utilities;
using NewAgePOS.ViewModels.Sale;
using NewAgePOSModels.Models;
using System.Collections.Generic;

namespace NewAgePOS.ViewModels.ViewComponent
{
  public class ItemListVCVM
  {
    public PathSourceEnum PathSource { get; set; }
    public List<ItemListViewModel> Items { get; set; } = new List<ItemListViewModel>();
    public List<RefundLineModel> Refunds { get; set; } = new List<RefundLineModel>();
  }
}
