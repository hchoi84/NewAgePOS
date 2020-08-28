using System.Collections.Generic;
using System.Threading.Tasks;
using NewAgePOSModels.Models;

namespace ChannelAdvisorLibrary
{
  public interface IChannelAdvisor
  {
    Task<List<ProductModel>> GetProductsByCodeAsync(List<string> codes);
  }
}