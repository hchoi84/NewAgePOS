using NewAgePOSModels.Models;
using System;
using System.Collections.Generic;

namespace NewAgePOSLibrary.Data
{
  public interface ISQLData
  {
    List<CustomerModel> Customers_GetByEmailOrPhone(string emailAddress, string phoneNumber);
    CustomerModel Customers_GetById(int id);
    CustomerModel Customers_GetBySaleId(int saleId);
    int Customers_Insert(string firstName, string lastName, string emailAddress, string phoneNumber);
    void Customers_Update(int id, string firstName, string lastName, string emailAddress, string phoneNumber);

    List<RefundDataModel> GetRefundReceiptData(int transactionId);

    ProductModel Products_GetByCode(string sku, string upc);
    List<ProductModel> Products_GetByParentSku(string parentSku);
    int Products_Insert(string sku, string upc, float cost, float price, string allName);
    void Products_Update(int productId, float cost, float price, string allName);


    int RefundLines_GetRefundQtyBySaleLineId(int saleLineId);
    void RefundLines_Insert(int saleId, int transactionId, int refundQty);

    void SaleLines_Delete(int id);
    List<SaleLineModel> SaleLines_GetBySaleId(int saleId);
    void SaleLines_Insert(int saleId, int productId, int qty);
    void SaleLines_Update(int id, int qty, float discPct);

    SaleModel Sales_GetById(int id);
    void Sales_CancelById(int id);
    bool Sales_GetStatus(int id);
    int Sales_Insert();
    void Sales_MarkComplete(int id);
    void Sales_UpdateCustomerId(int saleId, int customerId);
    void Sales_UpdateCustomerIdToGuest(int saleId);

    List<TransactionModel> Transactions_GetByDateRange(DateTime beginDate, DateTime endDate);
    List<TransactionModel> Transactions_GetBySaleId(int saleId);
    int Transactions_Insert(int saleId, float amount, string paymentType, string reason, string message);

    List<SaleSearchResultModel> SearchSales(int saleId, string lastName, string emailAddress, string phoneNumber);

    int Taxes_GetBySaleId(int saleId);
  }
}