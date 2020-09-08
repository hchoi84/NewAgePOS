using NewAgePOS.ViewModels.Sale;
using NewAgePOS.ViewModels.Shared;
using System.Collections.Generic;

namespace NewAgePOS.Utilities
{
  public interface IShare
  {
    List<CartViewModel> GenerateCartViewModel(int saleId);
    PriceSummaryViewModel GeneratePriceSummaryViewModel(int saleId);
  }
}