using System.Data;
using System.Reflection.Metadata;
using apitest;
using Dapper;
using Microsoft.Data.SqlClient;

namespace apitest
{
    public class DatadapperAsync
{
    private readonly IConfiguration _config;
    public DatadapperAsync(IConfiguration config)
    {
        _config = config;
    }

    public async Task<IEnumerable<T>> LoadDataAsync<T>(string sql)
    {
        using (IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
        {
            return await dbConnection.QueryAsync<T>(sql);
        }
    }

    public async Task<IEnumerable<T>> LoadDatatwoParamAsync<T>(string sql, object? parameters = null)
    {
        using (IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
        {
            if (parameters != null)
            {
                return await dbConnection.QueryAsync<T>(sql, parameters);
            }
            else
            {
                return await dbConnection.QueryAsync<T>(sql);
            }
        }
    }

    public async Task<T> LoadDataSingleAsync<T>(string sql, object? parameter = null)
    {
        using (IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
        {
            return await dbConnection.QuerySingleAsync<T>(sql, parameter);
        }
    }

    public async Task<bool> ExecuteSQLAsync(string sql, object? parameters = null)
    {
        using (IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
        {
            if (parameters != null)
            {
                return await dbConnection.ExecuteAsync(sql, parameters) > 0;
            }
            else
            {
                return await dbConnection.ExecuteAsync(sql) > 0;
            }
        }
    }

    public async Task<int> ExecuteSQLCountAsync(string sql)
    {
        using (IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
        {
            return await dbConnection.ExecuteScalarAsync<int>(sql);
        }
    }
}

}

