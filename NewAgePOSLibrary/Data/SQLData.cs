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

    public List<CustomerModel> Customers_GetByEmailOrPhone(string emailAddress, string phoneNumber)
    {
      string query = "SELECT * FROM dbo.Customers WHERE EmailAddress = @emailAddress OR PhoneNumber = @phoneNumber";
      return _sqlDb.LoadData<CustomerModel, dynamic>(query, new { emailAddress, phoneNumber }, connectionStringName, false);
    }

    public CustomerModel Customers_GetBySaleId(int saleId)
    {
      return _sqlDb.LoadData<CustomerModel, dynamic>("dbo.spCustomers_GetBySaleId",
                                                     new { saleId },
                                                     connectionStringName,
                                                     true).FirstOrDefault();
    }

    public CustomerModel Customers_GetById(int id)
    {
      string query = "SELECT * FROM dbo.Customers WHERE Id = @id";
      return _sqlDb.LoadData<CustomerModel, dynamic>(query, new { id }, connectionStringName, false).FirstOrDefault();
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

    public List<RefundDataModel> GetRefundReceiptData(int transactionId)
    {
      return _sqlDb.LoadData<RefundDataModel, dynamic>("dbo.spGetRefundReceiptData", new { transactionId }, connectionStringName, true);
    }

    public ProductModel Products_GetByCode(string sku, string upc)
    {
      string query = "SELECT * FROM dbo.Products WHERE Sku = @sku OR UPC = @upc;";
      return _sqlDb.LoadData<ProductModel, dynamic>(query, new { sku, upc }, connectionStringName, false).FirstOrDefault();
    }

    //public ProductModel Products_GetById(int id)
    //{
    //  string q = "SELECT * FROM dbo.Products WHERE Id = @id";
    //  return _sqlDb.LoadData<ProductModel, dynamic>(q, new { id }, connectionStringName, false).FirstOrDefault();
    //}

    public List<ProductModel> Products_GetByParentSku(string parentSku)
    {
      string query = "SELECT * FROM dbo.Products WHERE Sku LIKE @parentSku";
      parentSku += "%";
      return _sqlDb.LoadData<ProductModel, dynamic>(query, new { parentSku }, connectionStringName, false);
    }

    public int Products_Insert(string sku, string upc, float cost, float price, string allName)
    {
      string query = "INSERT INTO dbo.Products (Sku, Upc, Cost, Price, AllName, Source) OUTPUT inserted.Id VALUES (@sku, @upc, @cost, @price, @allName, 'API');";
      return _sqlDb.LoadData<int, dynamic>(query, new { sku, upc, cost, price, allName }, connectionStringName, false).FirstOrDefault();
    }

    public void Products_Update(int productId, float cost, float price, string allName)
    {
      string updateDate = DateTime.Now.ToShortDateString();
      string query = "UPDATE dbo.Products SET Cost = @cost, Price = @price, AllName = @allName, Updated = @updateDate WHERE Id = @productId;";
      _sqlDb.SaveData(query, new { productId, cost, price, allName, updateDate }, connectionStringName, false);
    }

    //public ProductModel Products_Manual_GetByCode(string sku, string upc)
    //{
    //  string query = "SELECT * FROM dbo.Products WHERE Source = 'Manual' AND Sku = @sku AND Upc = @upc";
    //  return _sqlDb.LoadData<ProductModel, dynamic>(query, new { sku, upc }, connectionStringName, false).FirstOrDefault();
    //}

    //public void Products_Manual_Insert(string sku, string upc, float cost, float price, string allName)
    //{
    //  string query = "INSERT INTO dbo.Products (Sku, Upc, Cost, Price, AllName, Source) VALUES (@sku, @upc, @cost, @price, @allName, 'Manual');";
    //  _sqlDb.SaveData(query, new { sku, upc, cost, price, allName }, connectionStringName, false);
    //}

    public List<RefundLineModel> RefundLines_GetBySaleLineId(int saleLineId)
    {
      string query = "SELECT * FROM dbo.RefundLines WHERE SaleLineId = @saleLineId";
      return _sqlDb.LoadData<RefundLineModel, dynamic>(query, new { saleLineId }, connectionStringName, false);
    }

    public void RefundLines_Insert(int saleLineId, int refundQty)
    {
      _sqlDb.SaveData("dbo.spRefundLines_Insert", new { saleLineId, refundQty }, connectionStringName, true);
    }

    public void RefundLines_SubtractQty(int id, int subtractQty)
    {
      _sqlDb.SaveData("dbo.spRefundLines_SubtractQty", new { id, subtractQty }, connectionStringName, true);
    }

    public void RefundLines_MarkComplete(int id, int transactionId)
    {
      string query = "UPDATE dbo.RefundLines SET TransactionId = @transactionId WHERE Id = @id";
      _sqlDb.SaveData(query, new { id, transactionId }, connectionStringName, false);
    }

    public void SaleLines_Delete(int id)
    {
      string query = "DELETE FROM dbo.SaleLines WHERE id = @id";

      _sqlDb.SaveData(query, new { id },
                      connectionStringName, false);
    }

    public List<SaleLineModel> SaleLines_GetBySaleId(int saleId)
    {
      return _sqlDb.LoadData<SaleLineModel, dynamic>("spSaleLines_GetBySaleId",
                                                     new { saleId },
                                                     connectionStringName, true);
    }

    public void SaleLines_Insert(int saleId, int productId, int qty)
    {
      _sqlDb.SaveData("dbo.spSaleLines_Insert",
                      new { saleId, productId, qty },
                      connectionStringName, true);
    }

    public void SaleLines_Update(int id, int qty, float discPct)
    {
      _sqlDb.SaveData("dbo.spSaleLines_Update",
                      new { id, qty, discPct },
                      connectionStringName, true);
    }

    public void Sales_CancelById(int id)
    {
      _sqlDb.SaveData("dbo.spSales_CancelById", new { id }, connectionStringName, true);
    }

    public SaleModel Sales_GetById(int id)
    {
      string query = "SELECT * FROM dbo.Sales WHERE Id = @id;";

      return _sqlDb.LoadData<SaleModel, dynamic>(query, new { id }, connectionStringName, false).FirstOrDefault();
    }

    public bool Sales_GetStatus(int id)
    {
      string query = "SELECT IsComplete FROM dbo.Sales WHERE Id = @id";
      return _sqlDb.LoadData<bool, dynamic>(query, new { id }, connectionStringName, false).FirstOrDefault();
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

    public void Sales_UpdateCustomerIdToGuest(int saleId)
    {
      _sqlDb.SaveData("dbo.spSales_UpdateCustomerIdToGuest",
                      new { saleId },
                      connectionStringName, true);
    }

    public List<TransactionModel> Transactions_GetBySaleId(int saleId)
    {
      string query = "SELECT * FROM dbo.Transactions WHERE SaleId = @saleId";

      return _sqlDb.LoadData<TransactionModel, dynamic>(query, new { saleId }, connectionStringName, false);
    }

    public List<TransactionModel> Transactions_GetByDateRange(DateTime beginDate, DateTime endDate)
    {
      string q = "SELECT * FROM dbo.Transactions WHERE Created >= @beginDate AND Created <= @endDate;";
      return _sqlDb.LoadData<TransactionModel, dynamic>(q,
                                                        new { beginDate = beginDate.ToShortDateString(), endDate = endDate.ToShortDateString() },
                                                        connectionStringName,
                                                        false);
    }

    public int Transactions_Insert(int saleId, float amount, string paymentType, string reason, string message)
    {
      return _sqlDb.LoadData<int, dynamic>("dbo.spTransactions_Insert",
                      new { saleId, amount, paymentType, reason, message },
                      connectionStringName, true).FirstOrDefault();
    }

    public int Taxes_GetBySaleId(int saleId)
    {
      return _sqlDb.LoadData<int, dynamic>("dbo.spTaxes_GetBySaleId",
                                           new { saleId },
                                           connectionStringName, true).FirstOrDefault();
    }

    public List<SaleSearchResultModel> SearchSales(int saleId, string lastName, string emailAddress, string phoneNumber)
    {
      return _sqlDb.LoadData<SaleSearchResultModel, dynamic>("dbo.spSearchSales", new { saleId, lastName, emailAddress, phoneNumber }, connectionStringName, true);
    }
  }
}
