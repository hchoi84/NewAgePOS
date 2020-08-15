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

    public List<SaleSearchModel> Sales_GetById(string id)
    {
      return _sqlDb.LoadData<SaleSearchModel, dynamic>(
        "dbo.spSales_GetBySaleId",
        new { id },
        connectionStringName, true);
    }

    public List<SaleSearchModel> Sales_GetByLastName(string lastName)
    {
      return _sqlDb.LoadData<SaleSearchModel, dynamic>(
        "dbo.spSales_GetByLastName",
        new { lastName },
        connectionStringName, true);
    }

    public List<SaleSearchModel> Sales_GetByEmailAddress(string emailAddress)
    {
      return _sqlDb.LoadData<SaleSearchModel, dynamic>(
        "dbo.spSales_GetByEmailAddress",
        new { emailAddress },
        connectionStringName, true);
    }

    public List<SaleSearchModel> Sales_GetByPhoneNumber(string phoneNumber)
    {
      return _sqlDb.LoadData<SaleSearchModel, dynamic>(
        "dbo.spSales_GetByPhoneNumber",
        new { phoneNumber },
        connectionStringName, true);
    }

    public int Sales_Insert()
    {
      return _sqlDb.LoadData<int, dynamic>(
        "dbo.spSales_Insert",
        new { },
        connectionStringName, true).First();
    }

    public List<SaleLineModel> SaleLines_GetBySaleId(int saleId)
    {
      return _sqlDb.LoadData<SaleLineModel, dynamic>(
        "dbo.spSaleLines_GetBySaleId",
        new { saleId },
        connectionStringName, true);
    }

    public void SaleLines_Update(int id, int qty, int discAmt, int discPct)
    {
      _sqlDb.SaveData(
        "spSaleLines_Update", 
        new { id, qty, discAmt, discPct }, 
        connectionStringName, true);
    }

    public int Products_GetByValues(string sku, string upc, float cost, float price, string allName)
    {
      return _sqlDb.LoadData<int, dynamic>(
        "dbo.spProducts_GetByValues",
        new { sku, upc, cost, price, allName },
        connectionStringName, true).First();
    }

    public void SaleLines_Insert(int saleId, int productId, int qty, int discAmt, int discPct, int refundQty)
    {
      _sqlDb.SaveData(
        "dbo.spSaleLines_Insert",
        new { saleId, productId, qty, discAmt, discPct, refundQty },
        connectionStringName, true);
    }

    public void SaleLines_Delete(int id)
    {
      _sqlDb.SaveData(
        $"DELETE FROM dbo.SaleLines WHERE id = { id };",
        new { },
        connectionStringName, false);
    }
  }
}
