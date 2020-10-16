using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ChannelAdvisorLibrary
{
  public interface IChannelAdvisor
  {
    Task<IEnumerable<JObject>> GetProductsByCodeAsync(IEnumerable<string> codes);
  }
}