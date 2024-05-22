using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace apitest
{
    public class CheckId : ControllerBase
    {
        private readonly IConfiguration _config;
        public CheckId(IConfiguration config)
        {
            _config = config;
        }

        public ObjectResult? checkAuthToken()
        {
            try
            {
                getUserId();
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
            return null;
        }


        public int getUserId()
        {
            string? accessToken = HttpContext.Request.Headers["Authorization"];
            if (accessToken != null && accessToken.StartsWith("Bearer "))
            {
                accessToken = accessToken.Substring("Bearer ".Length);
            }
            accessToken = accessToken?.Trim();
            int userId = 0;

            string sql0 = @"SELECT UserId From dbo.Tokens Where TokenValue= '" + accessToken + "'";
            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                SqlCommand command = new SqlCommand(sql0, conn);
                object result = command.ExecuteScalar();
                if (result != null)
                {
                    userId = Convert.ToInt32(result);
                    return userId;
                }
                conn.Close();
            }
            throw new Exception("Can't get user id");
        }

    }
}