using NewAgePOSModels.Securities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChannelAdvisorLibrary
{
  public class ChannelAdvisor : IChannelAdvisor
  {
    private void EstablishConnection()
    {
      if (Secrets.TokenExpireDateTime < DateTime.Now || Secrets.TokenExpireDateTime == null)
      {
        string auth = string.Concat(Secrets.ApplicationId, ":", Secrets.SharedSecret);
        byte[] authBytes = Encoding.ASCII.GetBytes(auth);
        string encodedAuth = Convert.ToBase64String(authBytes);
        string authorization = string.Concat("Basic ", encodedAuth);

        HttpRequestMessage request = new HttpRequestMessage
        {
          RequestUri = new Uri(Secrets.TokenUrl),
          Method = HttpMethod.Post,
          Headers = {
            { HttpRequestHeader.Authorization.ToString(), authorization },
            { HttpRequestHeader.ContentType.ToString(), CAStrings.appForm },
          },
          Content = new StringContent($"grant_type=refresh_token&refresh_token={ Secrets.RefreshToken }", Encoding.UTF8, CAStrings.appJson),
        };

        HttpClient client = new HttpClient();
        HttpResponseMessage response = client.SendAsync(request).Result;
        HttpContent content = response.Content;
        string json = content.ReadAsStringAsync().Result;
        JObject result = JObject.Parse(json);
        Secrets.AccessToken = result[CAStrings.accessToken].ToString();
        Secrets.TokenExpireDateTime = DateTime.Now.AddSeconds(Convert.ToDouble(result[CAStrings.expiresIn]) - Secrets.TokenExpireBuffer);
      }
    }

    private async Task<List<JObject>> GetProductsAsync(string filter, string expand, string select)
    {
      EstablishConnection();

      string reqUri = $"https://api.channeladvisor.com/v1/Products?access_token={ Secrets.AccessToken }";

      if (!string.IsNullOrWhiteSpace(filter)) reqUri += $"&$filter={ filter }";
      if (!string.IsNullOrWhiteSpace(expand)) reqUri += $"&$expand={ expand }";
      if (!string.IsNullOrWhiteSpace(select)) reqUri += $"&$select={ select }";

      List<JObject> jObjects = new List<JObject>();

      while (reqUri != null)
      {
        string result;

        using (HttpClient client = new HttpClient())
        {
          HttpResponseMessage response = await client.GetAsync(reqUri);
          HttpContent content = response.Content;
          result = await content.ReadAsStringAsync();
        }

        JObject jObject = JObject.Parse(result);

        if (jObject["error"] != null)
        {
          throw new Exception(jObject["error"]["message"].ToString());
        }

        reqUri = (string)jObject[CAStrings.odataNextLink];

        foreach (JObject item in jObject["value"]) jObjects.Add(item);
      }

      return jObjects;
    }

    public async Task<List<JObject>> GetProductsByCodeAsync(List<string> codes)
    {
      string expand = "Attributes";
      string select = "Sku,Upc,Cost,WarehouseLocation";
      List<string> filterContents = new List<string>();
      List<JObject> jObjects = new List<JObject>();

      foreach (var code in codes)
      {
        if (code.Length == 7) filterContents.Add($"ParentSku eq '{ code }'");
        else if (code.Contains('_')) filterContents.Add($"Sku eq '{ code }'");
        else filterContents.Add($"Upc eq '{ code }'");
      }

      while (filterContents.Count > 0)
      {
        bool isMoreThan9 = filterContents.Count > 9;
        int x = isMoreThan9 ? 9 : filterContents.Count;

        List<string> first9 = filterContents.GetRange(0, x).ToList();
        filterContents.RemoveRange(0, x);

        string filter = $"ProfileId eq { Secrets.MainProfileId } and ({ string.Join(" or ", first9) })";

        jObjects = await GetProductsAsync(filter, expand, select);
      }

      return jObjects;
    }


    public async Task<IEnumerable<JObject>> GetProductsByCodeAsync(IEnumerable<string> codes)
    {
      string expand = "Attributes";
      string select = "Sku,Upc,Cost,WarehouseLocation";
      List<string> filterContents = new List<string>();
      List<JObject> jObjects = new List<JObject>();

      foreach (var code in codes)
      {
        if (code.Length == 7) filterContents.Add($"ParentSku eq '{ code }'");
        else if (code.Contains('_')) filterContents.Add($"Sku eq '{ code }'");
        else filterContents.Add($"Upc eq '{ code }'");
      }

      while (filterContents.Count > 0)
      {
        bool isMoreThan9 = filterContents.Count > 9;
        int x = isMoreThan9 ? 9 : filterContents.Count;

        List<string> first9 = filterContents.GetRange(0, x).ToList();
        filterContents.RemoveRange(0, x);

        string filter = $"ProfileId eq { Secrets.MainProfileId } and ({ string.Join(" or ", first9) })";

        jObjects.AddRange(await GetProductsAsync(filter, expand, select));
      }

      return jObjects;
    }
  }
}
