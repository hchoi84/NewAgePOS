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

    public int Sales_Insert()
    {
      return _sqlDb.LoadData<int, dynamic>("dbo.spSales_Insert", new { }, connectionStringName, true).FirstOrDefault();
    }

    public List<SaleLineModel> SaleLines_GetBySaleId(int saleId)
    {
      return _sqlDb.LoadData<SaleLineModel, dynamic>("spSaleLines_GetBySaleId", new { saleId }, connectionStringName, true);
    }

    public int Taxes_GetBySaleId(int saleId)
    {
      return _sqlDb.LoadData<int, dynamic>("dbo.spTaxes_GetBySaleId", new { saleId }, connectionStringName, true).FirstOrDefault();
    }

    public void SaleLines_Update(int id, int qty, int discAmt, int discPct)
    {
      _sqlDb.SaveData("dbo.spSaleLines_Update", new { id, qty, discAmt, discPct }, connectionStringName, true);
    }

    public int Products_GetByValues(string sku, string upc, float cost, float price, string allName)
    {
      return _sqlDb.LoadData<int, dynamic>("dbo.spProducts_GetByValues", new { sku, upc, cost, price, allName }, connectionStringName, true).FirstOrDefault();
    }

    public void SaleLines_Insert(int saleId, int productId, int qty)
    {
      _sqlDb.SaveData("dbo.spSaleLines_Insert", new { saleId, productId, qty }, connectionStringName, true);
    }

    public void SaleLines_Delete(int id)
    {
      string query = "DELETE FROM dbo.Taxes WHERE id = @id";
      _sqlDb.SaveData(query, new { id }, connectionStringName, false);
    }

    public CustomerModel Customers_GetBySaleId(int saleId)
    {
      return _sqlDb.LoadData<CustomerModel, dynamic>("dbo.spCustomers_GetBySaleId", new { saleId }, connectionStringName, true).FirstOrDefault();
    }

    public void Sales_UpdateCustomerIdToGuest(int saleId)
    {
      _sqlDb.SaveData("dbo.spSales_UpdateCustomerIdToGuest", new { saleId }, connectionStringName, true);
    }

    public int Customers_Insert(string firstName, string lastName, string emailAddress, string phoneNumber)
    {
      return _sqlDb.LoadData<int, dynamic>("dbo.spCustomers_Insert", new { firstName, lastName, emailAddress, phoneNumber }, connectionStringName, true).FirstOrDefault();
    }

    public void Sales_UpdateCustomerId(int saleId, int customerId)
    {
      string query = "UPDATE dbo.Sales SET CustomerId = @customerId WHERE Id = @saleId";
      _sqlDb.SaveData(query, new { saleId, customerId }, connectionStringName, false);
    }
  }
}
