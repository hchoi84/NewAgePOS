using NewAgePOSLibrary.Databases;
using NewAgePOSModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewAgePOSLibrary.Data
{
  public class SQLData : ISQLData
  {
    private const string connectionStringName = "SqlDb";
    private readonly ISQLDataAccess _sqlDb;

    public SQLData(ISQLDataAccess sqlDb)
    {
      _sqlDb = sqlDb;
    }

    #region Customers
    public CustomerModel Customers_GetByEmailAddress(string emailAddress)
    {
      string query = "SELECT * FROM dbo.Customers WHERE EmailAddress = @emailAddress";
      return _sqlDb.LoadData<CustomerModel, dynamic>(query, new { emailAddress }, connectionStringName, false).FirstOrDefault();
    }

    public CustomerModel Customers_GetById(int id)
    {
      string query = "SELECT * FROM dbo.Customers WHERE Id = @id";
      return _sqlDb.LoadData<CustomerModel, dynamic>(query, new { id }, connectionStringName, false).FirstOrDefault();
    }

    public List<CustomerModel> Customers_GetByLastName(string lastName)
    {
      string query = "SELECT * FROM dbo.Customers WHERE LastName = @lastName";
      return _sqlDb.LoadData<CustomerModel, dynamic>(query, new { lastName }, connectionStringName, false);
    }

    public CustomerModel Customers_GetByPhoneNumber(string phoneNumber)
    {
      string query = "SELECT * FROM dbo.Customers WHERE PhoneNumber = @phoneNumber";
      return _sqlDb.LoadData<CustomerModel, dynamic>(query, new { phoneNumber }, connectionStringName, false).FirstOrDefault();
    }

    public CustomerModel Customers_GetBySaleId(int saleId)
    {
      return _sqlDb.LoadData<CustomerModel, dynamic>("dbo.spCustomers_GetBySaleId",
                                                     new { saleId },
                                                     connectionStringName,
                                                     true).FirstOrDefault();
    }

    public CustomerModel Customers_GetByTransactionId(int transactionId)
    {
      return _sqlDb.LoadData<CustomerModel, dynamic>("dbo.spCustomers_GetByTransactionId", new { transactionId }, connectionStringName, true).FirstOrDefault();
    }

    public int Customers_Insert(string firstName, string lastName, string emailAddress, string phoneNumber)
    {
      return _sqlDb.LoadData<int, dynamic>("dbo.spCustomers_Insert",
                                           new { firstName, lastName, emailAddress, phoneNumber },
                                           connectionStringName,
                                           true).FirstOrDefault();
    }

    public void Customers_Update(int id, string firstName, string lastName, string emailAddress, string phoneNumber)
    {
      _sqlDb.SaveData("dbo.spCustomers_Update", new { id, firstName, lastName, emailAddress, phoneNumber }, connectionStringName, true);
    }
    #endregion

    #region GiftCards
    public void GiftCards_Delete(int id)
    {
      string query = "DELETE FROM dbo.GiftCards WHERE Id = @id";
      _sqlDb.SaveData(query, new { id }, connectionStringName, false);
    }

    public GiftCardModel GiftCards_GetByCode(string code)
    {
      string query = "SELECT * FROM dbo.GiftCards WHERE Code = @code";
      return _sqlDb.LoadData<GiftCardModel, dynamic>(query, new { code }, connectionStringName, false).FirstOrDefault();
    }

    public GiftCardModel GiftCards_GetById(int id)
    {
      string query = "SELECT * FROM dbo.GiftCards WHERE Id = @id";
      return _sqlDb.LoadData<GiftCardModel, dynamic>(query, new { id }, connectionStringName, false).FirstOrDefault();
    }

    public List<GiftCardModel> GiftCards_GetBySaleId(int saleId)
    {
      return _sqlDb.LoadData<GiftCardModel, dynamic>("dbo.spGiftCards_GetBySaleId", new { saleId }, connectionStringName, true);
    }

    public int GiftCards_Insert(string code, float amount) =>
     _sqlDb.LoadData<int, dynamic>("dbo.spGiftCards_Insert", new { code, amount }, connectionStringName, true).FirstOrDefault();

    public void GiftCards_Update(int id, float amount)
    {
      string updateDate = DateTime.Now.ToShortDateString();
      string query = "UPDATE dbo.GiftCards SET Amount = @amount, Updated = @updateDate WHERE Id = @id";
      _sqlDb.SaveData(query, new { id, amount, updateDate }, connectionStringName, false);
    }
    #endregion

    #region Messages

    #endregion

    #region Products
    public ProductModel Products_GetByCode(string sku, string upc)
    {
      string query = "SELECT * FROM dbo.Products WHERE Sku = @sku OR UPC = @upc;";
      return _sqlDb.LoadData<ProductModel, dynamic>(query, new { sku, upc }, connectionStringName, false).FirstOrDefault();
    }

    public ProductModel Products_GetById(int id)
    {
      string query = "SELECT * FROM dbo.Products WHERE Id = @id";
      return _sqlDb.LoadData<ProductModel, dynamic>(query, new { id }, connectionStringName, false).FirstOrDefault();
    }

    public List<ProductModel> Products_GetBySaleId(int saleId)
    {
      return _sqlDb.LoadData<ProductModel, dynamic>("dbo.spProducts_GetBySaleId", new { saleId }, connectionStringName, true);
    }

    public int Products_Insert(string sku, string upc, float cost, float price, string allName)
    {
      string query = "INSERT INTO dbo.Products (Sku, Upc, Cost, Price, AllName) OUTPUT inserted.Id VALUES (@sku, @upc, @cost, @price, @allName);";
      return _sqlDb.LoadData<int, dynamic>(query, new { sku, upc, cost, price, allName }, connectionStringName, false).FirstOrDefault();
    }

    public void Products_Update(int productId, float cost, float price, string allName)
    {
      string updateDate = DateTime.Now.ToShortDateString();
      string query = "UPDATE dbo.Products SET Cost = @cost, Price = @price, AllName = @allName, Updated = @updateDate WHERE Id = @productId;";
      _sqlDb.SaveData(query, new { productId, cost, price, allName, updateDate }, connectionStringName, false);
    }
    #endregion

    #region RefundLines
    public List<RefundLineModel> RefundLines_GetBySaleId(int saleId)
    {
      return _sqlDb.LoadData<RefundLineModel, dynamic>("dbo.spRefundLines_GetBySaleId", new { saleId }, connectionStringName, true);
    }

    public List<RefundLineModel> RefundLines_GetByTransactionId(int transactionId)
    {
      string query = "SELECT * FROM dbo.RefundLines WHERE TransactionId = @transactionId;";
      return _sqlDb.LoadData<RefundLineModel, dynamic>(query, new { transactionId }, connectionStringName, false);
    }

    public void RefundLines_Insert(int saleLineId, int qty)
    {
      string query = "INSERT INTO dbo.RefundLines (SaleLineId, Qty) VALUES (@saleLineId, @qty)";
      _sqlDb.SaveData(query, new { saleLineId, qty }, connectionStringName, false);
    }

    public void RefundLines_MarkComplete(int id, int transactionId)
    {
      string query = "UPDATE dbo.RefundLines SET TransactionId = @transactionId WHERE Id = @id";
      _sqlDb.SaveData(query, new { id, transactionId }, connectionStringName, false);
    }

    public void RefundLines_Update(int id, int qty)
    {
      string query = "UPDATE dbo.RefundLines SET Qty = @qty WHERE Id = @id";
      _sqlDb.SaveData(query, new { id, qty }, connectionStringName, false);
    }
    #endregion

    #region SaleLines
    public void SaleLines_Delete(int id)
    {
      string query = "DELETE FROM dbo.SaleLines WHERE id = @id";

      _sqlDb.SaveData(query, new { id }, connectionStringName, false);
    }

    public List<SaleLineModel> SaleLines_GetBySaleId(int saleId)
    {
      string query = @"SELECT * FROM dbo.SaleLines WHERE SaleId = @saleId;";
      return _sqlDb.LoadData<SaleLineModel, dynamic>(query, new { saleId }, connectionStringName, false);
    }

    public void SaleLines_Insert(int saleId, float tradeInValue, float tradeInQty)
    {
      string query = "INSERT INTO dbo.SaleLines (SaleId, Cost, Price, Qty) VALUES (@saleId, 0, @tradeInValue, @tradeInQty);";
      _sqlDb.SaveData(query, new { saleId, tradeInValue, tradeInQty }, connectionStringName, false);
    }

    public void SaleLines_Insert(int saleId, int? productId, int? giftCardId, int qty)
    {
      _sqlDb.SaveData("dbo.spSaleLines_Insert",
                      new { saleId, productId, giftCardId, qty },
                      connectionStringName, true);
    }

    public void SaleLines_Update(int id, int qty, float discPct)
    {
      _sqlDb.SaveData("dbo.spSaleLines_Update",
                      new { id, qty, discPct },
                      connectionStringName, true);
    }
    #endregion

    #region Sales
    public void Sales_CancelById(int id)
    {
      _sqlDb.SaveData("dbo.spSales_CancelById", new { id }, connectionStringName, true);
    }

    public SaleModel Sales_GetById(int id)
    {
      string query = "SELECT * FROM dbo.Sales WHERE Id = @id;";

      return _sqlDb.LoadData<SaleModel, dynamic>(query, new { id }, connectionStringName, false).FirstOrDefault();
    }

    public List<SaleModel> Sales_GetByCustomerId(int customerId)
    {
      string query = "SELECT * FROM dbo.Sales WHERE CustomerId = @customerId";
      return _sqlDb.LoadData<SaleModel, dynamic>(query, new { customerId }, connectionStringName, false);
    }

    public int Sales_Insert()
    {
      return _sqlDb.LoadData<int, dynamic>("dbo.spSales_Insert",
                                           new { },
                                           connectionStringName,
                                           true).FirstOrDefault();
    }

    public void Sales_MarkComplete(int id)
    {
      string query = "UPDATE dbo.Sales SET IsComplete = 1 WHERE Id = @id";

      _sqlDb.SaveData(query, new { id }, connectionStringName, false);
    }

    public void Sales_UpdateCustomerId(int saleId, int customerId)
    {
      string query = "UPDATE dbo.Sales SET CustomerId = @customerId WHERE Id = @saleId";

      _sqlDb.SaveData(query,
                      new { saleId, customerId },
                      connectionStringName, false);
    }
    #endregion

    #region Taxes
    public TaxModel Taxes_GetBySaleId(int saleId)
    {
      return _sqlDb.LoadData<TaxModel, dynamic>(
        "dbo.spTaxes_GetBySaleId", new { saleId }, connectionStringName, true)
        .FirstOrDefault();
    }
    #endregion

    #region Transactions
    public void Transactions_DeleteById(int id)
    {
      string query = "DELETE FROM dbo.Transactions WHERE Id = @id";
      _sqlDb.SaveData(query, new { id }, connectionStringName, false);
    }
    public TransactionModel Transactions_GetById(int id)
    {
      string query = "SELECT * FROM dbo.Transactions WHERE Id = @id;";
      return _sqlDb.LoadData<TransactionModel, dynamic>(query, new { id }, connectionStringName, false).FirstOrDefault();
    }

    public List<TransactionModel> Transactions_GetByDateRange(DateTime beginDate, DateTime endDate)
    {
      string query = "SELECT * FROM dbo.Transactions WHERE Created >= @beginDate AND Created <= @endDate;";
      return _sqlDb.LoadData<TransactionModel, dynamic>(
        query,
        new { beginDate = beginDate.ToShortDateString(), endDate = endDate.ToShortDateString() },
        connectionStringName,
        false);
    }

    public List<TransactionModel> Transactions_GetBySaleId(int saleId)
    {
      string query = "SELECT * FROM dbo.Transactions WHERE SaleId = @saleId";
      return _sqlDb.LoadData<TransactionModel, dynamic>(query, new { saleId }, connectionStringName, false);
    }

    public int Transactions_Insert(int saleId, int? giftCardId, float amount, MethodEnum method, TypeEnum type)
    {
      return _sqlDb.LoadData<int, dynamic>("dbo.spTransactions_Insert",
                    new { saleId, giftCardId, amount, method = method.ToString(), type = type.ToString() },
                    connectionStringName, true).FirstOrDefault();
    }

    public void Transactions_UpdateAmount(int id, float newAmt)
    {
      string query = "UPDATE dbo.Transactions SET Amount = @newAmt WHERE Id = @id";
      _sqlDb.SaveData(query, new { id, newAmt }, connectionStringName, false);
    }
    #endregion
  }
}
