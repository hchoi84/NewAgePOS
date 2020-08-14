using NewAgePOSLibrary.Models;
using System.Collections.Generic;

namespace NewAgePOSLibrary.Data
{
  public interface ISQLData
  {
    int CreateSale();
    List<SaleLineModel> GetSaleLinesBySaleId(int saleId);
    List<SaleSearchModel> GetSalesByEmailAddress(string emailAddress);
    List<SaleSearchModel> GetSalesByLastName(string lastName);
    List<SaleSearchModel> GetSalesByPhoneNumber(string phoneNumber);
    List<SaleSearchModel> GetSalesBySaleId(string id);
  }
}