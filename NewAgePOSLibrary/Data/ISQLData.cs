using NewAgePOSLibrary.Models;
using System.Collections.Generic;

namespace NewAgePOSLibrary.Data
{
  public interface ISQLData
  {
    int Sales_Insert();
    List<SaleLineModel> SaleLines_GetBySaleId(int saleId);
    List<SaleSearchModel> Sales_GetByEmailAddress(string emailAddress);
    List<SaleSearchModel> Sales_GetByLastName(string lastName);
    List<SaleSearchModel> Sales_GetByPhoneNumber(string phoneNumber);
    List<SaleSearchModel> Sales_GetById(string id);
    void SaleLines_Update(int id, int qty, int discAmt, int discPct);
    int Products_GetByValues(string sku, string upc, float cost, float price, string allName);
    void SaleLines_Insert(int saleId, int productId, int qty, int discAmt, int discPct, int refundQty);
    void SaleLines_Delete(int id);
  }
}