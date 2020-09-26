using NewAgePOSModels.Securities;
using System;

namespace NewAgePOSModels.Utilities
{
  public static class DateTimeExtensions
  {
    public static DateTime UTCtoPST(this DateTime dateTime)
    {
      if (!Secrets.DBIsLocal)
      {
        TimeZoneInfo pst = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
        return TimeZoneInfo.ConvertTimeFromUtc(dateTime, pst);
      }

      return dateTime;
    }

    public static DateTime PSTtoUTC(this DateTime dateTime)
    {
      if (!Secrets.DBIsLocal)
      {
        TimeZoneInfo pst = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
        return TimeZoneInfo.ConvertTimeToUtc(dateTime, pst);
      }

      return dateTime;
    }
  }
}
