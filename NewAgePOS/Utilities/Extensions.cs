using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewAgePOS.Utilities
{
  public static class Extensions
  {
    public static void SetObject(this ISession session, string key, object value)
    {
      session.SetString(key, JsonConvert.SerializeObject(value));
    }

    public static T GetObject<T>(this ISession session, string key)
    {
      var value = session.GetString(key);

      return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
    }

    public static Dictionary<string, int> CountIt(this string codes)
    {
      IEnumerable<string> myCodes = codes.Split(Environment.NewLine).Select(c => c.Trim().ToUpper());
      Dictionary<string, int> codeCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

      foreach (var myCode in myCodes)
      {
        if (string.IsNullOrEmpty(myCode)) continue;

        if (codeCount.ContainsKey(myCode))
          codeCount[myCode]++;
        else
          codeCount.Add(myCode, 1);
      }

      return codeCount;
    }
  }
}
