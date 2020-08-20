using NewAgePOSLibrary.Databases;
using NewAgePOSLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    public int Customers_GetByEmailOrPhone(string emailAddress, string phoneNumber)
    {
      string query = "SELECT COUNT(*) FROM dbo.Customers WHERE EmailAddress = @emailAddress OR PhoneNumber = @phoneNumber";
      return _sqlDb.LoadData<int, dynamic>(query, new { emailAddress, phoneNumber }, connectionStringName, false).FirstOrDefault();
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

    public List<RefundDataModel> GetRefundReceiptData(int saleTransactionId)
    {
      return _sqlDb.LoadData<RefundDataModel, dynamic>("dbo.spGetRefundReceiptData", new { saleTransactionId }, connectionStringName, true);
    }

    public int Products_GetByValues(string sku, string upc, float cost, float price, string allName)
    {
      return _sqlDb.LoadData<int, dynamic>("dbo.spProducts_GetByValues",
                                           new { sku, upc, cost, price, allName },
                                           connectionStringName,
                                           true).FirstOrDefault();
    }

    public int RefundLines_GetRefundQtyBySaleLineId(int saleLineId)
    {
      string query = "SELECT SUM(Qty) FROM dbo.RefundLines WHERE SaleLineId = @saleLineId";
      return _sqlDb.LoadData<int, dynamic>(query, new { saleLineId }, connectionStringName, false).FirstOrDefault();
    }

    public void RefundLines_Insert(int saleId, int saleTransactionId, int refundQty)
    {
      string q = "INSERT INTO dbo.RefundLines (SaleLineId, SaleTransactionId, Qty) VALUES (@saleId, @saleTransactionId, @refundQty);";
      _sqlDb.SaveData(q, new { saleId, saleTransactionId, refundQty }, connectionStringName, false);
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

    public void SaleLines_Update(int id, int qty, int discAmt, int discPct)
    {
      _sqlDb.SaveData("dbo.spSaleLines_Update",
                      new { id, qty, discAmt, discPct },
                      connectionStringName, true);
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

    public List<SaleTransactionModel> SaleTransaction_GetBySaleId(int saleId)
    {
      string query = "SELECT * FROM dbo.SaleTransactions WHERE SaleId = @saleId";

      return _sqlDb.LoadData<SaleTransactionModel, dynamic>(query, new { saleId }, connectionStringName, false);
    }
    
    public int SaleTransaction_Insert(int saleId, float amount, string paymentType, string reason, string message)
    {
      return _sqlDb.LoadData<int, dynamic>("dbo.spSaleTransaction_Insert",
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
