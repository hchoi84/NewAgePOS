using Dapper;
using Microsoft.Extensions.Configuration;
using NewAgePOSModels.Securities;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace NewAgePOSLibrary.Databases
{
  public class SQLDataAccess : ISQLDataAccess
  {
    private readonly IConfiguration _config;

    private string _connectionString;

    public SQLDataAccess(IConfiguration config)
    {
      _config = config;
      _connectionString = $"Server=posdb;Database=posdb;User Id={ Secrets.DBUser };Password={ Secrets.DBPassword }";
    }

    public List<T> LoadData<T, U>(string sqlStatement,
                                  U parameters,
                                  string connectionStringName,
                                  bool isStoredProcedure = false)
    {
      string connectionString;

      if (Secrets.DBIsLocal)
        connectionString = _config.GetConnectionString(connectionStringName);
      else
        connectionString = _connectionString;

      CommandType commandType = CommandType.Text;

      if (isStoredProcedure == true)
      {
        commandType = CommandType.StoredProcedure;
      }

      using (IDbConnection connection = new SqlConnection(connectionString))
      {
        List<T> rows = connection.Query<T>(sqlStatement, parameters, commandType: commandType).ToList();
        return rows;
      }
    }

    public void SaveData<T>(string sqlStatement,
                            T parameters,
                            string connectionStringName,
                            bool isStoredProcedure = false)
    {
      string connectionString;

      if (Secrets.DBIsLocal)
        connectionString = _config.GetConnectionString(connectionStringName);
      else
        connectionString = _connectionString;

      CommandType commandType = CommandType.Text;

      if (isStoredProcedure == true)
      {
        commandType = CommandType.StoredProcedure;
      }

      using (IDbConnection connection = new SqlConnection(connectionString))
      {
        connection.Execute(sqlStatement, parameters, commandType: commandType);
      }
    }
  }
}
