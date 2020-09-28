using NewAgePOSModels.Models;
using System;
using System.Collections.Generic;

namespace NewAgePOSLibrary.Data
{
  public interface ISQLData
  {
    #region Customers
    CustomerModel Customers_GetByEmailAddress(string emailAddress);
    CustomerModel Customers_GetById(int id);
    List<CustomerModel> Customers_GetByLastName(string lastName);
    CustomerModel Customers_GetByPhoneNumber(string phoneNumber);
    CustomerModel Customers_GetBySaleId(int saleId);
    CustomerModel Customers_GetByTransactionId(int transactionId);
    int Customers_Insert(string firstName, string lastName, string emailAddress, string phoneNumber);
    void Customers_Update(int id, string firstName, string lastName, string emailAddress, string phoneNumber);
    #endregion

    #region GiftCards
    void GiftCards_Delete(int id);
    GiftCardModel GiftCards_GetByCode(string code);
    GiftCardModel GiftCards_GetById(int id);
    List<GiftCardModel> GiftCards_GetBySaleId(int saleId);
    int GiftCards_Insert(string code, float amount);
    void GiftCards_Update(int id, float amount);
    #endregion

    #region Messages
    void Messages_Delete(int id);
    void Messages_Edit(int id, string message);
    List<MessageModel> Messages_GetBySaleId(int saleId);
    int Messages_Insert(int saleId, string message);
    #endregion

    #region Products
    ProductModel Products_GetByCode(string sku, string upc);
    ProductModel Products_GetById(int id);
    List<ProductModel> Products_GetBySaleId(int saleId);
    int Products_Insert(string sku, string upc, float cost, float price, string allName);
    void Products_Update(ProductModel product);
    #endregion

    #region RefundLines
    void RefundLines_Delete(int id);
    List<RefundLineModel> RefundLines_GetBySaleId(int saleId);
    List<RefundLineModel> RefundLines_GetByTransactionId(int transactionId);
    void RefundLines_Insert(int saleLineId, int qty);
    void RefundLines_MarkComplete(int id, int transactionId);
    void RefundLines_Update(int id, int qty);
    #endregion

    #region SaleLines
    void SaleLines_Delete(int id);
    List<SaleLineModel> SaleLines_GetBySaleId(int saleId);
    void SaleLines_Insert(int saleId, float tradeInValue, float tradeInQty);
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
    #endregion

    #region Taxes
    TaxModel Taxes_GetBySaleId(int saleId);
    #endregion

    #region Transactions
    List<TransactionModel> Transactions_GetByDateRange(DateTime beginDate, DateTime endDate);
    TransactionModel Transactions_GetById(int id);
    List<TransactionModel> Transactions_GetBySaleId(int saleId);
    int Transactions_Insert(int saleId, int? giftCardId, float amount, MethodEnum method, TypeEnum type);
    void Transactions_UpdateAmount(int id, float newAmt);
    void Transactions_DeleteById(int id);
    int Messages_GetCountBySaleId(int saleId);
    List<SaleModel> Sales_GetPending();
    #endregion
  }
}