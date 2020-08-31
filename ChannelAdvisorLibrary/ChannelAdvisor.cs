using NewAgePOSModels.Securities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NewAgePOSModels.Models;

namespace ChannelAdvisorLibrary
{
  public class ChannelAdvisor : IChannelAdvisor
  {
    private readonly string _accessToken = "access_token";
    private readonly string _expiresIn = "expires_in";
    private readonly string _appForm = "application/x-www-form-urlencoded";
    private readonly string _appJson = "application/json";
    private readonly string _odataNextLink = "@odata.nextLink";
    private readonly string _sku = "Sku";
    private readonly string _upc = "UPC";
    private readonly string _cost = "Cost";
    private readonly string _bcprice = "BCPrice";
    private readonly string _attributes = "Attributes";
    private readonly string _name = "Name";
    private readonly string _allName = "All Name";
    private readonly string _Value = "Value";

    private readonly List<string> _labelNames = new List<string>
    {
      "Closeout", "Discount", "MAPNoShow", "MAPShow", "NPIP"
    };

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
            { HttpRequestHeader.ContentType.ToString(), _appForm },
          },
          Content = new StringContent($"grant_type=refresh_token&refresh_token={ Secrets.RefreshToken }", Encoding.UTF8, _appJson),
        };

        HttpClient client = new HttpClient();
        HttpResponseMessage response = client.SendAsync(request).Result;
        HttpContent content = response.Content;
        string json = content.ReadAsStringAsync().Result;
        JObject result = JObject.Parse(json);
        Secrets.AccessToken = result[_accessToken].ToString();
        Secrets.TokenExpireDateTime = DateTime.Now.AddSeconds(Convert.ToDouble(result[_expiresIn]) - Secrets.TokenExpireBuffer);
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

        reqUri = (string)jObject[_odataNextLink];

        foreach (JObject item in jObject["value"]) jObjects.Add(item);
      }

      return jObjects;
    }

    public async Task<List<ProductModel>> GetProductsByCodeAsync(List<string> codes)
    {
      string expand = "Attributes";
      string select = "Sku,Upc,Cost";
      List<ProductModel> products = new List<ProductModel>();
      List<string> filterContents = new List<string>();

      foreach (var code in codes)
      {
        if (code.Contains('_')) filterContents.Add($"Sku eq '{ code }'");
        else filterContents.Add($"Upc eq '{ code }'");
      }

      while (filterContents.Count > 0)
      {
        bool isMoreThan9 = filterContents.Count > 9;
        int x = isMoreThan9 ? 9 : filterContents.Count;

        List<string> first9 = filterContents.GetRange(0, x).ToList();
        filterContents.RemoveRange(0, x);

        string filter = $"ProfileId eq { Secrets.MainProfileId } and ({ string.Join(" or ", first9) })";

        List<JObject> jObjects = await GetProductsAsync(filter, expand, select);

        foreach (var item in jObjects)
        {
          ProductModel product = new ProductModel()
          {
            Sku = item[_sku].ToString(),
            Upc = item[_upc].ToString(),
            AllName = item[_attributes]
            .FirstOrDefault(i => i[_name].ToString() == _allName)[_Value]
            .ToString(),
            Cost = String.IsNullOrEmpty(item[_cost].ToString()) ? 0 : item[_cost].ToObject<float>(),
            Price = item[_attributes]
            .FirstOrDefault(i => i[_name].ToString() == _bcprice)[_Value]
            .ToObject<float>()
          };

          products.Add(product);
        }
      }

      return products;
    }
  }
}
