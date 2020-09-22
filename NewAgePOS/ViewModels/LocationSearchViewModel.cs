using NewAgePOSModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewAgePOS.ViewModels
{
  public class LocationSearchViewModel : ProductModel
  {
    public int RequestQty { get; set; }
    public List<ProductLocationModel> Locations { get; set; } = new List<ProductLocationModel>();
  }
}
