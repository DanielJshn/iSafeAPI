using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;


namespace apitest
{
    public class CheckId : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<CheckId> _logger;

        public CheckId(IConfiguration config, ILogger<CheckId> logger)
        {
            _config = config;
            _logger = logger;
        }

        private ObjectResult? checkAuthToken()
        {
            try
            {
                int userId = getUserId();
                return Ok(userId);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt.");
                return StatusCode(401, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking auth token.");
                return StatusCode(500, "Internal server error");
            }
        }

        private int getUserId()
        {
            string? accessToken = HttpContext.Request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(accessToken) || !accessToken.StartsWith("Bearer "))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }

            accessToken = accessToken.Substring("Bearer ".Length).Trim();

            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                string sqlQuery = "SELECT UserId FROM dbo.Tokens WHERE TokenValue = @TokenValue";
                SqlCommand command = new SqlCommand(sqlQuery, conn);
                command.Parameters.AddWithValue("@TokenValue", accessToken);

                conn.Open();
                object result = command.ExecuteScalar();
                if (result != null && int.TryParse(result.ToString(), out int userId))
                {
                    return userId;
                }
                conn.Close();
            }
            
            throw new UnauthorizedAccessException("Cannot get user ID");
        }

        public int ValidateAndGetUserId()
        {
            try
            {
                return getUserId();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt.");
                throw new UnauthorizedAccessException("Invalid token");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while validating the auth token.");
                throw new Exception("An error occurred while validating the auth token.");
            }
        }
    }
}