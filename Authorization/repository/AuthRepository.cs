using System.Data;
using System.Security.Cryptography;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace apitest
{
    class AuthRepository : IAuthRepository
    {
        private readonly DatadapperAsync _dapper;

        private readonly AuthHelp _authHelp;

        public AuthRepository(DatadapperAsync dapper, AuthHelp authHelp)
        {
            _dapper = dapper;
            _authHelp = authHelp;
        }


        public async Task CheckUserAsync(UserForRegistrationDto userForRegistration)
        {
            string sqlCheckUserExists = "SELECT email FROM dbo.Tokens WHERE email = @UserEmail";
            IEnumerable<string> existingUsers = await _dapper.LoadDatatwoParamAsync<string>(sqlCheckUserExists, new { UserEmail = userForRegistration.Email });
            if (existingUsers.Any())
            {
                throw new InvalidOperationException("User with this email already exists.");
            }
        }

        public async Task<string> RegistrEndInsertAsync(UserForRegistrationDto userForRegistration)
        {
            byte[] passwordSalt = new byte[128 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }

            byte[] passwordHash = _authHelp.GetPasswordHash(userForRegistration.Password, passwordSalt);

            string token =  _authHelp.CreateToken(userForRegistration.Email);

            string sqlAddAuth = @"
        INSERT INTO dbo.Tokens  ([Email], [PasswordHash], [PasswordSalt], [TokenValue]) 
        VALUES (@Email, @PasswordHash, @PasswordSalt, @TokenValue)";

            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@Email", SqlDbType.NVarChar) { Value = userForRegistration.Email },
        new SqlParameter("@PasswordHash", SqlDbType.VarBinary) { Value = passwordHash },
        new SqlParameter("@PasswordSalt", SqlDbType.VarBinary) { Value = passwordSalt },
        new SqlParameter("@TokenValue", SqlDbType.NVarChar) { Value = token }
    };

            if (await _dapper.ExecuteSqlWithParametersAsync(sqlAddAuth, sqlParameters))
            {
                return token;
            }
            else
            {
                throw new Exception("Failed to register user.");
            }
        }

        public async Task<string> CheckEmailAsync(UserForLoginDto userForLogin)
        {
            string sqlForToken = @"SELECT [UserId], [TokenValue] FROM dbo.Tokens WHERE Email = @UserEmail";

            var token = (await _dapper.LoadDatatwoParamAsync<Token>(sqlForToken, new { UserEmail = userForLogin.Email })).FirstOrDefault();

            if (token == null)
            {
                throw new Exception("Incorrect Email!");
            }
            return token.TokenValue;
        }

        public async Task CheckPasswordAsync(UserForLoginDto userForLogin)
        {
            string sqlForHashAndSalt = @"SELECT [PasswordHash], [PasswordSalt] FROM dbo.Tokens WHERE Email = @UserEmail";

            var userForConfirmation = (await _dapper.LoadDatatwoParamAsync<UserForLoginConfirmationDto>(sqlForHashAndSalt, new { UserEmail = userForLogin.Email })).FirstOrDefault();

            if (userForConfirmation == null)
            {
                throw new Exception("Incorrect Email!");
            }
            byte[] passwordHash =  _authHelp.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

            if (!passwordHash.SequenceEqual(userForConfirmation.PasswordHash))
            {
                throw new Exception("Incorrect password!");
            }
        }

        public async Task<byte[]?> GetSaltForUserIdAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("The correct user ID is not specified");
            }

            string sql = @"SELECT PasswordSalt FROM Tokens WHERE UserId = @UserId";
            return await _dapper.ExecuteSQLbyteAsync(sql, new { UserId = userId });
        }

        public async Task<byte[]?> GetHashForUserIdAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("The correct user ID is not specified");
            }

            string sql = @"SELECT PasswordHash FROM Tokens WHERE UserId = @UserId";
            return await _dapper.ExecuteSQLbyteAsync(sql, new { UserId = userId });
        }

        public async Task ChangeUserPasswordAsync(int userId, byte[] newPasswordHash)
        {
            if (userId <= 0 || newPasswordHash == null || newPasswordHash.Length == 0)
            {
                throw new ArgumentException("The correct data for changing the password is not specified");
            }

            string sqlUpdatePassword = @"UPDATE Tokens SET PasswordHash = @NewPasswordHash WHERE UserId = @UserId";
            await _dapper.ExecuteSQLAsync(sqlUpdatePassword, new { NewPasswordHash = newPasswordHash, UserId = userId });
        }

        public async Task DeletePasswordDataAsync(List<Password> resultPasswords, int userId)
        {
            foreach (Password password in resultPasswords)
            {
                string sql = "DELETE FROM AdditionalFields WHERE passwordId = @PasswordId";
                await _dapper.ExecuteSQLAsync(sql, new { PasswordId = password.id });
            }
            string sqlPassword = "DELETE FROM Passwords WHERE UserId = @UserId";
            await _dapper.ExecuteSQLAsync(sqlPassword, new { UserId = userId });
        }

        public async Task DeleteUserAsync(int id)
        {
            string sqlUser = "DELETE FROM dbo.Tokens WHERE UserId = @id";

            if (!await _dapper.ExecuteSQLAsync(sqlUser, new { id }))
            {
                throw new Exception("Failed to delete User");
            }
        }

    }
}