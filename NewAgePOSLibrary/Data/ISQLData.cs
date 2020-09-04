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
    int Products_Insert(string sku, string upc, float cost, float price, string allName);
    void Products_Update(int productId, float cost, float price, string allName);

    List<RefundLineModel> RefundLines_GetBySaleLineId(int saleLineId);
    void RefundLines_Insert(int saleLineId, int refundQty);

    void SaleLines_Delete(int id);
    List<SaleLineModel> SaleLines_GetBySaleId(int saleId);
    void SaleLines_Insert(int saleId, int? productId, int? giftCardId, int qty);
    void SaleLines_Update(int id, int qty, float discPct);

    SaleModel Sales_GetById(int id);
    void Sales_CancelById(int id);
    int Sales_Insert();
    void Sales_MarkComplete(int id);
    void Sales_UpdateCustomerId(int saleId, int customerId);
    void Sales_UpdateCustomerIdToGuest(int saleId);

    List<TransactionModel> Transactions_GetByDateRange(DateTime beginDate, DateTime endDate);
    List<TransactionModel> Transactions_GetBySaleId(int saleId);
    int Transactions_Insert(int saleId, int? giftCardId, float amount, string method, string type, string message);

    List<SaleSearchResultModel> SearchSales(int saleId, string lastName, string emailAddress, string phoneNumber);

    int Taxes_GetBySaleId(int saleId);
    void RefundLines_SubtractQty(int id, int subtractQty);
    void RefundLines_MarkComplete(int id, int transactionId);
    GiftCardModel GiftCards_GetByCode(string code);
    int GiftCards_Insert(string code, float amount);
    void GiftCards_Update(int id, float amount);
    void GiftCards_Delete(int id);
    ProductModel Products_GetById(int id);
    GiftCardModel GiftCards_GetById(int id);
  }
}