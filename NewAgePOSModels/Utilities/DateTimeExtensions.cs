using NewAgePOSModels.Securities;
using System;

namespace NewAgePOS.Utilities
{
  public static class DateTimeExtensions
  {
    public static DateTime USTtoPST(this DateTime dateTime)
    {
      if (!Secrets.DBIsLocal)
      {
        TimeZoneInfo pst = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
        return TimeZoneInfo.ConvertTimeFromUtc(dateTime, pst);
      }

      return dateTime;
    }
  }
}
