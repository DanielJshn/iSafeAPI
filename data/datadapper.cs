using System.Data;
using System.Reflection.Metadata;
using apitest;
using Dapper;
using Microsoft.Data.SqlClient;

namespace api.Controllers
{
    public class Datadapper
    {
        private readonly IConfiguration _config;
        public Datadapper(IConfiguration config)
        {
            _config = config;
        }
        public IEnumerable<T> LoadData<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Query<T>(sql);
        }
        public IEnumerable<T> LoadDatatwoParam<T>(string sql, object? parameters = null)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            if (parameters != null)
            {
                return dbConnection.Query<T>(sql, parameters);
            }
            else
            {
                return dbConnection.Query<T>(sql);
            }
        }


        public T LoadDataSingle<T>(string sql, object? parameter = null)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.QuerySingle<T>(sql);
        }
        public bool ExecuteSQL(string sql, object? parameters = null)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            if (parameters != null)
            {
                return dbConnection.Execute(sql, parameters) > 0;
            }
            else
            {
                return dbConnection.Execute(sql) > 0;
            }
        }

        public int ExecuteSQLCount(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql);
        }

        public bool ExecuteSqlWithParameters(string sql, List<SqlParameter> parameters)
        {
            SqlCommand commandWithParams = new SqlCommand(sql);

            foreach (SqlParameter parameter in parameters)
            {
                commandWithParams.Parameters.Add(parameter);
            }

            SqlConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            dbConnection.Open();

            commandWithParams.Connection = dbConnection;

            int rowsAffected = commandWithParams.ExecuteNonQuery();

            dbConnection.Close();

            return rowsAffected > 0;
        }
        public int GetUserIdByEmail(string email)
        {
            string sqlGetUserId = "SELECT UserId FROM dbo.Tokens WHERE Email = @Email";
            SqlConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            int userId = dbConnection.QueryFirstOrDefault<int>(sqlGetUserId, new { Email = email });
            return userId;
        }

        public void SaveToken(Token token)
        {
            SqlConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            string sqlInsertToken = @"
                INSERT INTO Tokens ( TokenValue) 
                VALUES ( @TokenValue)";

            dbConnection.Execute(sqlInsertToken, new
            {

                TokenValue = token.TokenValue

            });
        }


    }
}

