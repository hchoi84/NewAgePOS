using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkuVaultLibrary
{
  public interface ISkuVault
  {
    Task RemoveProducts(Dictionary<string, int> productsToRemove);
  }
}