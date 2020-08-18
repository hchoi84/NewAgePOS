using NewAgePOSLibrary.Models;
using System.Collections.Generic;

namespace NewAgePOSLibrary.Data
{
  public interface ISQLData
  {
    CustomerModel Customers_GetBySaleId(int saleId);
    int Customers_Insert(string firstName, string lastName, string emailAddress, string phoneNumber);
    int Products_GetByValues(string sku, string upc, float cost, float price, string allName);
    void SaleLines_Delete(int id);
    List<SaleLineModel> SaleLines_GetBySaleId(int saleId);
    void SaleLines_Insert(int saleId, int productId, int qty);
    void SaleLines_Update(int id, int qty, int discAmt, int discPct);
    int Sales_Insert();
    void Sales_UpdateCustomerId(int saleId, int customerId);
    void Sales_UpdateCustomerIdToGuest(int saleId);
    int Taxes_GetBySaleId(int saleId);
  }
}