using NewAgePOSModels.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkuVaultLibrary
{
  public interface ISkuVault
  {
    Task<JObject> AddItemBulkAsync(List<AddRemoveItemBulkModel> itemsToAdd);
    Task<List<ProductLocationModel>> GetInventoryLocationsAsync(List<string> codes, bool isSKU);
    Task<JObject> RemoveItemBulkAsync(List<AddRemoveItemBulkModel> itemsToRemove);
  }
}