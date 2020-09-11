using NewAgePOS.ViewModels.Shared;

namespace NewAgePOS.Utilities
{
  public interface IShare
  {
    PriceSummaryViewModel GeneratePriceSummaryViewModel(int saleId);
  }
}