using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkuVaultLibrary
{
  public interface ISkuVault
  {
    Task<JObject> RemoveProducts(Dictionary<string, int> productsToRemove);
  }
}