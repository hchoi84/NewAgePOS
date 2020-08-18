using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkuVaultLibrary.Securities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SkuVaultLibrary
{
  public class SkuVault
  {
    public SkuVault()
    {
      GetTokensAsync().Wait();
    }

    #region strings
    private readonly string _appjson = "application/json";
    private readonly string _tenantToken = "TenantToken";
    private readonly string _userToken = "UserToken";
    private readonly string _locationCode = "LocationCode";
    private readonly string _store = "STORE";
    private readonly string _quantity = "Quantity";
    private readonly string _items = "Items";
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


  }
}
