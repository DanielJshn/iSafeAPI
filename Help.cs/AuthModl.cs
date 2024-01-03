using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Caching.Memory;

namespace apitest
{

    public class AuthHelp
    {
        private readonly IConfiguration _config;
        
        public AuthHelp(IConfiguration config)
        {
            _config = config;
          
        }
        

        

        public byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value +
                Convert.ToBase64String(passwordSalt);

            return KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 1000000,
                numBytesRequested: 256 / 8
            );
        }

        public string CreateToken(string userEmail)
        {
            Claim[] claims = new Claim[]
            {
        new Claim("Email", userEmail)
            };

            string? TokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;

            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenKeyString != null ? TokenKeyString : ""));

            SigningCredentials credentials = new SigningCredentials(tokenKey, SecurityAlgorithms.HmacSha512Signature);

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddDays(1)
            };

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            SecurityToken token = handler.CreateToken(descriptor);



            return handler.WriteToken(token);
        }

        public string? GetUserEmailFromDatabase(int userId)
        {
            string sql = "SELECT Email FROM Tokens WHERE UserId = @UserId";

            using (SqlConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                string? userEmail = dbConnection.QueryFirstOrDefault<string>(sql, new { UserId = userId });
                return userEmail;
            }
        }
        public string GenerateNewToken(int userId)
        {
            string? userEmail = GetUserEmailFromDatabase(userId);

            if (userEmail == null)
            {

                throw new Exception("Адрес электронной почты пользователя не найден.");
            }

            Claim[] claims = new Claim[]
            {
        new Claim("Email", userEmail)
            };

            string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;
            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKeyString ?? ""));

            SigningCredentials credentials = new SigningCredentials(tokenKey, SecurityAlgorithms.HmacSha512Signature);

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddDays(1)
            };

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            SecurityToken token = handler.CreateToken(descriptor);

            return handler.WriteToken(token);
        }
        public int GetUserIdFromToken(string? accessToken)
        {
            int userId = 0;

            if (accessToken != null && accessToken.StartsWith("Bearer "))
            {
                accessToken = accessToken.Substring("Bearer ".Length);
            }

            accessToken = accessToken?.Trim();

            string sql = @"SELECT UserId FROM dbo.Tokens WHERE TokenValue = @AccessToken";

            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                conn.Open();

                SqlCommand command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue("@AccessToken", accessToken);

                object result = command.ExecuteScalar();
                if (result != null)
                {
                    userId = Convert.ToInt32(result);
                }

                conn.Close();
            }
            Console.WriteLine(userId);
            return userId;
        }
        public bool UpdateTokenValueInDatabase(int userId, string newToken)
        {
            try
            {
                string? connectionString = _config.GetConnectionString("DefaultConnection");
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sqlUpdate = "UPDATE Tokens SET TokenValue = @NewToken WHERE UserId = @UserId";
                    SqlCommand updateCommand = new SqlCommand(sqlUpdate, conn);
                    updateCommand.Parameters.AddWithValue("@NewToken", newToken);
                    updateCommand.Parameters.AddWithValue("@UserId", userId);

                    int rowsAffected = updateCommand.ExecuteNonQuery();

                    conn.Close();

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating token in database: {ex.Message}");
                return false;
            }
        }




    }
}






