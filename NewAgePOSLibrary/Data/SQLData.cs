using NewAgePOSLibrary.Databases;
using NewAgePOSModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewAgePOSLibrary.Data
{
  public class SQLData : ISQLData
  {
    private const string _connectionStringName = "SqlDb";
    private readonly ISQLDataAccess _sqlDb;

    public SQLData(ISQLDataAccess sqlDb)
    {
      _sqlDb = sqlDb;
    }

    #region Customers
    public CustomerModel Customers_GetByEmailAddress(string emailAddress)
    {
      string query = "SELECT * FROM dbo.Customers WHERE EmailAddress = @emailAddress";
      return _sqlDb.LoadData<CustomerModel, dynamic>(query, new { emailAddress }, _connectionStringName, false).FirstOrDefault();
    }

    public CustomerModel Customers_GetById(int id)
    {
      string query = "SELECT * FROM dbo.Customers WHERE Id = @id";
      return _sqlDb.LoadData<CustomerModel, dynamic>(query, new { id }, _connectionStringName, false).FirstOrDefault();
    }

    public List<CustomerModel> Customers_GetByLastName(string lastName)
    {
      string query = "SELECT * FROM dbo.Customers WHERE LastName = @lastName";
      return _sqlDb.LoadData<CustomerModel, dynamic>(query, new { lastName }, _connectionStringName, false);
    }

    public CustomerModel Customers_GetByPhoneNumber(string phoneNumber)
    {
      string query = "SELECT * FROM dbo.Customers WHERE PhoneNumber = @phoneNumber";
      return _sqlDb.LoadData<CustomerModel, dynamic>(query, new { phoneNumber }, _connectionStringName, false).FirstOrDefault();
    }

    public CustomerModel Customers_GetBySaleId(int saleId)
    {
      return _sqlDb.LoadData<CustomerModel, dynamic>("dbo.spCustomers_GetBySaleId",
                                                     new { saleId },
                                                     _connectionStringName,
                                                     true).FirstOrDefault();
    }

    public CustomerModel Customers_GetByTransactionId(int transactionId)
    {
      return _sqlDb.LoadData<CustomerModel, dynamic>("dbo.spCustomers_GetByTransactionId", new { transactionId }, _connectionStringName, true).FirstOrDefault();
    }

    public int Customers_Insert(string firstName, string lastName, string emailAddress, string phoneNumber)
    {
      string query = "INSERT INTO dbo.Customers (FirstName, LastName, EmailAddress, PhoneNumber) OUTPUT inserted.Id VALUES (@firstName, @lastName, @emailAddress, @phoneNumber)";
      return _sqlDb.LoadData<int, dynamic>(query, new { firstName, lastName, emailAddress, phoneNumber }, _connectionStringName, false).FirstOrDefault();
    }

    public void Customers_Update(int id, string firstName, string lastName, string emailAddress, string phoneNumber)
    {
      DateTime updated = DateTime.Now;
      string query = "UPDATE dbo.Customers SET FirstName = @firstName, LastName = @lastName, EmailAddress = @emailAddress, PhoneNumber = @phoneNumber, Updated = @updated WHERE Id = @id";
      _sqlDb.SaveData(query, new { id, firstName, lastName, emailAddress, phoneNumber, updated }, _connectionStringName, false);
    }
    #endregion

    #region GiftCards
    public void GiftCards_Delete(int id)
    {
      string query = "DELETE FROM dbo.GiftCards WHERE Id = @id";
      _sqlDb.SaveData(query, new { id }, _connectionStringName, false);
    }

    public GiftCardModel GiftCards_GetByCode(string code)
    {
      string query = "SELECT * FROM dbo.GiftCards WHERE Code = @code";
      return _sqlDb.LoadData<GiftCardModel, dynamic>(query, new { code }, _connectionStringName, false).FirstOrDefault();
    }

    public GiftCardModel GiftCards_GetById(int id)
    {
      string query = "SELECT * FROM dbo.GiftCards WHERE Id = @id";
      return _sqlDb.LoadData<GiftCardModel, dynamic>(query, new { id }, _connectionStringName, false).FirstOrDefault();
    }

    public List<GiftCardModel> GiftCards_GetBySaleId(int saleId)
    {
      return _sqlDb.LoadData<GiftCardModel, dynamic>("dbo.spGiftCards_GetBySaleId", new { saleId }, _connectionStringName, true);
    }

    public int GiftCards_Insert(string code, float amount) =>
     _sqlDb.LoadData<int, dynamic>("dbo.spGiftCards_Insert", new { code, amount }, _connectionStringName, true).FirstOrDefault();

    public void GiftCards_Update(int id, float amount)
    {
      DateTime updated = DateTime.Now;
      string query = "UPDATE dbo.GiftCards SET Amount = @amount, Updated = @updated WHERE Id = @id";
      _sqlDb.SaveData(query, new { id, amount, updated }, _connectionStringName, false);
    }
    #endregion

    #region Messages
    public void Messages_Delete(int id)
    {
      string query = "DELETE FROM dbo.Messages WHERE Id = @id";
      _sqlDb.SaveData(query, new { id }, _connectionStringName, false);
    }

    public void Messages_Edit(int id, string message)
    {
      string query = "UPDATE dbo.Messages SET Message = @message WHERE Id = @id";
      _sqlDb.SaveData(query, new { id, message }, _connectionStringName, false);
    }

    public List<MessageModel> Messages_GetBySaleId(int saleId)
    {
      return _sqlDb.LoadData<MessageModel, dynamic>("dbo.spMessages_GetBySaleId", new { saleId }, _connectionStringName, true);
    }

    public int Messages_GetCountBySaleId(int saleId)
    {
      string query = "SELECT COUNT(Id) FROM dbo.Messages WHERE SaleId = @saleId";
      return _sqlDb.LoadData<int, dynamic>(query, new { saleId }, _connectionStringName, false).FirstOrDefault();
    }

    public int Messages_Insert(int saleId, string message)
    {
      string query = "INSERT INTO dbo.Messages (SaleId, Message) VALUES (@saleId, @message)";
      return _sqlDb.LoadData<int, dynamic>(query, new { saleId, message }, _connectionStringName, false).FirstOrDefault();
    }
    #endregion

    #region Products
    public ProductModel Products_GetByCode(string sku, string upc)
    {
      string query = "SELECT * FROM dbo.Products WHERE Sku = @sku OR UPC = @upc;";
      return _sqlDb.LoadData<ProductModel, dynamic>(query, new { sku, upc }, _connectionStringName, false).FirstOrDefault();
    }

    public ProductModel Products_GetById(int id)
    {
      string query = "SELECT * FROM dbo.Products WHERE Id = @id";
      return _sqlDb.LoadData<ProductModel, dynamic>(query, new { id }, _connectionStringName, false).FirstOrDefault();
    }

    public List<ProductModel> Products_GetBySaleId(int saleId)
    {
      return _sqlDb.LoadData<ProductModel, dynamic>("dbo.spProducts_GetBySaleId", new { saleId }, _connectionStringName, true);
    }

    public int Products_Insert(string sku, string upc, float cost, float price, string allName)
    {
      string query = "INSERT INTO dbo.Products (Sku, Upc, Cost, Price, AllName) OUTPUT inserted.Id VALUES (@sku, @upc, @cost, @price, @allName);";
      return _sqlDb.LoadData<int, dynamic>(query, new { sku, upc, cost, price, allName }, _connectionStringName, false).FirstOrDefault();
    }

    public void Products_Update(ProductModel product)
    {
      DateTime updated = DateTime.Now;
      string query = "UPDATE dbo.Products SET Sku = @sku, Upc = @upc, Cost = @cost, Price = @price, AllName = @allName, Updated = @updated WHERE Id = @id;";
      _sqlDb.SaveData(query, new { id = product.Id, sku = product.Sku, upc = product.Upc, cost = product.Cost, price = product.Price, allName = product.AllName, updated }, _connectionStringName, false);
    }
    #endregion

    #region RefundLines
    public void RefundLines_Delete(int id)
    {
      string query = "DELETE FROM dbo.RefundLines WHERE Id = @id";
      _sqlDb.SaveData(query, new { id }, _connectionStringName, false);
    }

    public List<RefundLineModel> RefundLines_GetBySaleId(int saleId)
    {
      return _sqlDb.LoadData<RefundLineModel, dynamic>("dbo.spRefundLines_GetBySaleId", new { saleId }, _connectionStringName, true);
    }

    public List<RefundLineModel> RefundLines_GetByTransactionId(int transactionId)
    {
      string query = "SELECT * FROM dbo.RefundLines WHERE TransactionId = @transactionId;";
      return _sqlDb.LoadData<RefundLineModel, dynamic>(query, new { transactionId }, _connectionStringName, false);
    }

    public void RefundLines_Insert(int saleLineId, int qty)
    {
      string query = "INSERT INTO dbo.RefundLines (SaleLineId, Qty) VALUES (@saleLineId, @qty)";
      _sqlDb.SaveData(query, new { saleLineId, qty }, _connectionStringName, false);
    }

    public void RefundLines_MarkComplete(int id, int transactionId)
    {
      string query = "UPDATE dbo.RefundLines SET TransactionId = @transactionId WHERE Id = @id";
      _sqlDb.SaveData(query, new { id, transactionId }, _connectionStringName, false);
    }

    public void RefundLines_Update(int id, int qty)
    {
      DateTime updated = DateTime.Now;
      string query = "UPDATE dbo.RefundLines SET Qty = @qty, Updated = @updated WHERE Id = @id";
      _sqlDb.SaveData(query, new { id, qty, updated }, _connectionStringName, false);
    }
    #endregion

    #region SaleLines
    public void SaleLines_Delete(int id)
    {
      string query = "DELETE FROM dbo.SaleLines WHERE id = @id";

      _sqlDb.SaveData(query, new { id }, _connectionStringName, false);
    }

    public List<SaleLineModel> SaleLines_GetBySaleId(int saleId)
    {
      string query = @"SELECT * FROM dbo.SaleLines WHERE SaleId = @saleId;";
      return _sqlDb.LoadData<SaleLineModel, dynamic>(query, new { saleId }, _connectionStringName, false);
    }

    public void SaleLines_Insert(int saleId, float tradeInValue, float tradeInQty)
    {
      string query = "INSERT INTO dbo.SaleLines (SaleId, Cost, Price, Qty) VALUES (@saleId, 0, @tradeInValue, @tradeInQty);";
      _sqlDb.SaveData(query, new { saleId, tradeInValue, tradeInQty }, _connectionStringName, false);
    }

    public void SaleLines_Insert(int saleId, int? productId, int? giftCardId, int qty)
    {
      _sqlDb.SaveData("dbo.spSaleLines_Insert",
                      new { saleId, productId, giftCardId, qty },
                      _connectionStringName, true);
    }

    public void SaleLines_Update(int id, int qty, float discPct)
    {
      DateTime updated = DateTime.Now;
      string query = "UPDATE dbo.SaleLines SET Qty = @qty, DiscPct = @discPct, Updated = @updated WHERE Id = @id";
      _sqlDb.SaveData(query, new { id, qty, discPct, updated }, _connectionStringName, false);
    }
    #endregion

    #region Sales
    public void Sales_CancelById(int id)
    {
      _sqlDb.SaveData("dbo.spSales_CancelById", new { id }, _connectionStringName, true);
    }

    public SaleModel Sales_GetById(int id)
    {
      string query = "SELECT * FROM dbo.Sales WHERE Id = @id;";

      return _sqlDb.LoadData<SaleModel, dynamic>(query, new { id }, _connectionStringName, false).FirstOrDefault();
    }

    public List<SaleModel> Sales_GetByCustomerId(int customerId)
    {
      string query = "SELECT * FROM dbo.Sales WHERE CustomerId = @customerId";
      return _sqlDb.LoadData<SaleModel, dynamic>(query, new { customerId }, _connectionStringName, false);
    }

    public List<SaleModel> Sales_GetPending()
    {
      string query = "SELECT * FROM dbo.Sales WHERE IsComplete = 0";
      return _sqlDb.LoadData<SaleModel, dynamic>(query, new { }, _connectionStringName, false);
    }

    public int Sales_Insert()
    {
      return _sqlDb.LoadData<int, dynamic>("dbo.spSales_Insert",
                                           new { },
                                           _connectionStringName,
                                           true).FirstOrDefault();
    }

    public void Sales_MarkComplete(int id)
    {
      DateTime updated = DateTime.Now;
      string query = "UPDATE dbo.Sales SET IsComplete = 1, Updated = @updated WHERE Id = @id";
      _sqlDb.SaveData(query, new { id, updated }, _connectionStringName, false);
    }

    public void Sales_UpdateCustomerId(int saleId, int customerId)
    {
      DateTime updated = DateTime.Now;
      string query = "UPDATE dbo.Sales SET CustomerId = @customerId, Updated = @updated WHERE Id = @saleId";
      _sqlDb.SaveData(query,
                      new { saleId, customerId, updated },
                      _connectionStringName, false);
    }
    #endregion

    #region Taxes
    public TaxModel Taxes_GetBySaleId(int saleId)
    {
      return _sqlDb.LoadData<TaxModel, dynamic>(
        "dbo.spTaxes_GetBySaleId", new { saleId }, _connectionStringName, true)
        .FirstOrDefault();
    }
    #endregion

    #region Transactions
    public void Transactions_DeleteById(int id)
    {
      string query = "DELETE FROM dbo.Transactions WHERE Id = @id";
      _sqlDb.SaveData(query, new { id }, _connectionStringName, false);
    }

    public TransactionModel Transactions_GetById(int id)
    {
      string query = "SELECT * FROM dbo.Transactions WHERE Id = @id;";
      return _sqlDb.LoadData<TransactionModel, dynamic>(query, new { id }, _connectionStringName, false).FirstOrDefault();
    }

    public List<TransactionModel> Transactions_GetByDateRange(DateTime beginDate, DateTime endDate)
    {
      return _sqlDb.LoadData<TransactionModel, dynamic>(
        "spTransactions_GetByDateRange",
        new { beginDate, endDate },
        _connectionStringName,
        true);
    }

    public List<TransactionModel> Transactions_GetBySaleId(int saleId)
    {
      string query = "SELECT * FROM dbo.Transactions WHERE SaleId = @saleId";
      return _sqlDb.LoadData<TransactionModel, dynamic>(query, new { saleId }, _connectionStringName, false);
    }

    public int Transactions_Insert(int saleId, int? giftCardId, float amount, MethodEnum method, TypeEnum type)
    {
      string query = "INSERT INTO dbo.Transactions (SaleId, GiftCardId, Amount, Method, Type) OUTPUT inserted.Id VALUES (@saleId, @giftCardId, @amount, @method, @type)";
      return _sqlDb.LoadData<int, dynamic>(query,
                    new { saleId, giftCardId, amount, method = method.ToString(), type = type.ToString() },
                    _connectionStringName, false).FirstOrDefault();
    }

    public void Transactions_UpdateAmount(int id, float newAmt)
    {
      DateTime updated = DateTime.Now;
      string query = "UPDATE dbo.Transactions SET Amount = @newAmt, Updated = @updated WHERE Id = @id";
      _sqlDb.SaveData(query, new { id, newAmt, updated }, _connectionStringName, false);
    }
    #endregion
  }
}
