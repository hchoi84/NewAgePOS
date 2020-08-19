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
    SaleModel Sales_GetById(int id);
    int Sales_Insert();
    void Sales_UpdateCustomerId(int saleId, int customerId);
    void Sales_UpdateCustomerIdToGuest(int saleId);
    List<SaleTransactionModel> SaleTransaction_GetBySaleId(int saleId);
    void SaleTransaction_Insert(int saleId, float amount, string paymentType, string reason, string message);
    List<SaleSearchResultModel> SearchSales(int saleId, string lastName, string emailAddress, string phoneNumber);
    int Taxes_GetBySaleId(int saleId);
  }
}