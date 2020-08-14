using ChannelAdvisorLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChannelAdvisorLibrary
{
  public interface IChannelAdvisor
  {
    Task<List<ProductModel>> GetProductsByCodeAsync(List<string> codes);
  }
}