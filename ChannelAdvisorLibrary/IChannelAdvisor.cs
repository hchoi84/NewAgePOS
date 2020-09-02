using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ChannelAdvisorLibrary
{
  public interface IChannelAdvisor
  {
    Task<List<JObject>> GetProductsByCodeAsync(List<string> codes);
  }
}