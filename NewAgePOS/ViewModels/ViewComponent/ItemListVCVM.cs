using NewAgePOS.Utilities;
using NewAgePOS.ViewModels.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewAgePOS.ViewModels.ViewComponent
{
  public class ItemListVCVM
  {
    public PathSourceEnum PathSource { get; set; }
    public List<ItemListViewModel> Items { get; set; } = new List<ItemListViewModel>();
  }
}
