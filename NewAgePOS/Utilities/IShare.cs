using NewAgePOS.ViewModels.Sale;
using NewAgePOS.ViewModels.Shared;
using System.Collections.Generic;

namespace NewAgePOS.Utilities
{
  public interface IShare
  {
    List<ItemListViewModel> GenerateCartViewModel(int saleId);
    PriceSummaryViewModel GeneratePriceSummaryViewModel(int saleId);
  }
}