using NewAgePOSModels.Models;
using System;
using System.Collections.Generic;

namespace NewAgePOSLibrary.Data
{
  public interface ISQLData
  {
    #region Customers
    List<CustomerModel> Customers_GetByEmailOrPhone(string emailAddress, string phoneNumber);
    CustomerModel Customers_GetByEmailAddress(string emailAddress);
    CustomerModel Customers_GetByPhoneNumber(string phoneNumber);
    List<CustomerModel> Customers_GetByLastName(string lastName);
    CustomerModel Customers_GetById(int id);
    CustomerModel Customers_GetBySaleId(int saleId);
    int Customers_Insert(string firstName, string lastName, string emailAddress, string phoneNumber);
    void Customers_Update(int id, string firstName, string lastName, string emailAddress, string phoneNumber);
    #endregion

    #region GiftCards
    void GiftCards_Delete(int id);
    GiftCardModel GiftCards_GetByCode(string code);
    GiftCardModel GiftCards_GetById(int id);
    int GiftCards_Insert(string code, float amount);
    void GiftCards_Update(int id, float amount);
    #endregion

    #region Products
    ProductModel Products_GetByCode(string sku, string upc);
    ProductModel Products_GetById(int id);
    int Products_Insert(string sku, string upc, float cost, float price, string allName);
    void Products_Update(int productId, float cost, float price, string allName);
    #endregion

    #region RefundLines
    List<RefundLineModel> RefundLines_GetBySaleLineId(int saleLineId);
    void RefundLines_Insert(int saleLineId, int refundQty);
    void RefundLines_SubtractQty(int id, int subtractQty);
    void RefundLines_MarkComplete(int id, int transactionId);
    #endregion

    #region SaleLines
    void SaleLines_Delete(int id);
    List<SaleLineModel> SaleLines_GetBySaleId(int saleId);
    void SaleLines_Insert(int saleId, int? productId, int? giftCardId, int qty);
    void SaleLines_Update(int id, int qty, float discPct);
    #endregion

    #region Sales
    void Sales_CancelById(int id);
    SaleModel Sales_GetById(int id);
    List<SaleModel> Sales_GetByCustomerId(int customerId);
    int Sales_Insert();
    void Sales_MarkComplete(int id);
    void Sales_UpdateCustomerId(int saleId, int customerId);
    void Sales_UpdateCustomerIdToGuest(int saleId);
    #endregion

    #region Taxes
    TaxModel Taxes_GetBySaleId(int saleId);
    #endregion

    #region Transactions
    List<TransactionModel> Transactions_GetByDateRange(DateTime beginDate, DateTime endDate);
    List<TransactionModel> Transactions_GetBySaleId(int saleId);
    int Transactions_Insert(int saleId, int? giftCardId, float amount, string method, string type, string message);
    #endregion

    List<RefundDataModel> GetRefundReceiptData(int transactionId);
    List<ProductModel> Products_GetBySaleId(int saleId);
    List<GiftCardModel> GiftCards_GetBySaleId(int saleId);
  }
}