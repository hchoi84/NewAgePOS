using NewAgePOSModels.Models;
using System.Collections.Generic;

namespace NewAgePOS.ViewModels
{
  public class LocationSearchViewModel : ProductModel
  {
    public int RequestQty { get; set; }
    public List<ProductLocationModel> Locations { get; set; } = new List<ProductLocationModel>();
  }
}
