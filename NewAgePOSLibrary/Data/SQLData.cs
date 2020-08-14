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

    public List<SaleSearchModel> GetSalesBySaleId(string id)
    {
      return _sqlDb.LoadData<SaleSearchModel, dynamic>(
        "dbo.spSales_GetBySaleId",
        new { id },
        connectionStringName,
        true);
    }

    public List<SaleSearchModel> GetSalesByLastName(string lastName)
    {
      return _sqlDb.LoadData<SaleSearchModel, dynamic>(
        "dbo.spSales_GetByLastName",
        new { lastName },
        connectionStringName,
        true);
    }

    public List<SaleSearchModel> GetSalesByEmailAddress(string emailAddress)
    {
      return _sqlDb.LoadData<SaleSearchModel, dynamic>(
        "dbo.spSales_GetByEmailAddress",
        new { emailAddress },
        connectionStringName,
        true);
    }

    public List<SaleSearchModel> GetSalesByPhoneNumber(string phoneNumber)
    {
      return _sqlDb.LoadData<SaleSearchModel, dynamic>(
        "dbo.spSales_GetByPhoneNumber",
        new { phoneNumber },
        connectionStringName,
        true);
    }

    public int CreateSale()
    {
      return _sqlDb.LoadData<int, dynamic>(
        "dbo.spSales_Insert",
        new { },
        connectionStringName,
        true).First();
    }

    public List<SaleLineModel> GetSaleLinesBySaleId(int saleId)
    {
      return _sqlDb.LoadData<SaleLineModel, dynamic>(
        "dbo.spSaleLines_GetBySaleId",
        new { saleId },
        connectionStringName,
        true);
    }
  }
}
