﻿using NewAgePOSModels.Models;
using NewAgePOSModels.Securities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SkuVaultLibrary
{
  public class SkuVault : ISkuVault
  {
    public SkuVault()
    {
      GetTokensAsync().Wait();
    }

    #region strings
    private readonly string _appjson = "application/json";
    private readonly string _tenantToken = "TenantToken";
    private readonly string _userToken = "UserToken";
    #endregion

    private async Task GetTokensAsync()
    {
      if (string.IsNullOrWhiteSpace(Secrets.TenantToken) ||
        string.IsNullOrWhiteSpace(Secrets.UserToken))
      {
        string reqUri = "https://app.skuvault.com/api/gettokens";

        string body = JsonConvert.SerializeObject(
          new { Secrets.Email, Secrets.Password });

        StringContent content = new StringContent(body, Encoding.UTF8, _appjson);

        JObject tokens = await PostDataAsync(reqUri, content);

        Secrets.TenantToken = tokens[_tenantToken].ToString();
        Secrets.UserToken = tokens[_userToken].ToString();
      }

      if (Secrets.WalnutWHID == 0)
      {
        await GetWarehouseIdsAsync();
      }
    }

    private async Task<JObject> PostDataAsync(string reqUri, StringContent content)
    {
      string result;

      using (HttpClient client = new HttpClient())
      {
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_appjson));
        HttpResponseMessage response = await client.PostAsync(reqUri, content);
        HttpContent httpContent = response.Content;
        result = await httpContent.ReadAsStringAsync();
      }

      JObject jObject = JObject.Parse(result);

      return jObject;
    }

    private async Task GetWarehouseIdsAsync()
    {
      string reqUri = "https://app.skuvault.com/api/inventory/getWarehouses";

      string body = JsonConvert.SerializeObject(new
      {
        PageNumber = 0,
        Secrets.TenantToken,
        Secrets.UserToken
      });

      StringContent content = new StringContent(body, Encoding.UTF8, _appjson);

      JObject jObject = await PostDataAsync(reqUri, content);

      JArray warehouses = jObject["Warehouses"].ToObject<JArray>();
      //Secrets.WalnutWHID = warehouses.FirstOrDefault(w => w["Code"].ToString() == "WALNUT")["Id"].ToObject<int>();
      Secrets.WalnutWHID = 4007;
      //Secrets.DropshipWHID = warehouses.FirstOrDefault(w => w["Code"].ToString() == "DROPSHIP")["Id"].ToObject<int>();
      Secrets.DropshipWHID = 4081;
    }

    public async Task<JObject> RemoveItemBulkAsync(List<AddRemoveItemBulkModel> itemsToRemove)
    {
      string reqUri = "https://app.skuvault.com/api/inventory/removeItemBulk";
      StringContent content = GenerateAddRemoveContent(itemsToRemove);
      return await PostDataAsync(reqUri, content);
    }

    public async Task<JObject> AddItemBulkAsync(List<AddRemoveItemBulkModel> itemsToAdd)
    {
      string reqUri = "https://app.skuvault.com/api/inventory/addItemBulk";
      StringContent content = GenerateAddRemoveContent(itemsToAdd);
      return await PostDataAsync(reqUri, content);
    }

    private StringContent GenerateAddRemoveContent(List<AddRemoveItemBulkModel> items)
    {
      List<object> products = new List<object>();

      foreach (var item in items)
      {
        if (item.Code.Contains("_"))
        {
          products.Add(new
          {
            Sku = item.Code,
            WarehouseId = Secrets.WalnutWHID,
            item.LocationCode,
            item.Quantity,
            item.Reason
          });
        }
        else
        {
          products.Add(new
          {
            item.Code,
            WarehouseId = Secrets.WalnutWHID,
            item.LocationCode,
            item.Quantity,
            item.Reason
          });
        }
      }

      string body = JsonConvert.SerializeObject(new
      {
        Items = products,
        Secrets.TenantToken,
        Secrets.UserToken
      });

      StringContent content = new StringContent(body, Encoding.UTF8, _appjson);
      return content;
    }

    public async Task<JObject> GetInventoryLocationsAsync(List<string> codes, bool isSKU)
    {
      string reqUri = "https://app.skuvault.com/api/inventory/getInventoryByLocation";
      string body;

      if (isSKU)
      {
        body = JsonConvert.SerializeObject(new
        {
          IsReturnByCodes = false,
          PageNumber = 0,
          PageSize = 1000,
          ProductSKUs = codes.ToArray(),
          Secrets.TenantToken,
          Secrets.UserToken
        });
      }
      else
      {
        body = JsonConvert.SerializeObject(new
        {
          IsReturnByCodes = true,
          PageNumber = 0,
          PageSize = 1000,
          ProductCodes = codes.ToArray(),
          Secrets.TenantToken,
          Secrets.UserToken
        });
      }

      StringContent content = new StringContent(body, Encoding.UTF8, _appjson);

      return await PostDataAsync(reqUri, content);
    }
  }
}
