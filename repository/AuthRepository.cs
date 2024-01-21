using System.Data;
using System.Security.Cryptography;
using api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace apitest
{
    class AuthRepository
    {
        private readonly Datadapper _dapper;
        private readonly HttpContext _context;
        private readonly AuthHelp _authHelp;
     
       

        public AuthRepository(Datadapper dapper, AuthHelp authHelp, HttpContext context )
        {
            _context = context;
            _dapper = dapper;
            _authHelp = authHelp;
           
            
        }

        public void CheckUser(UserForRegistrationDto userForRegistration)
        {
            string sqlCheckUserExists = "SELECT email FROM dbo.Tokens WHERE email = @UserEmail";
            IEnumerable<string> existingUsers = _dapper.LoadDatatwoParam<string>(sqlCheckUserExists, new { UserEmail = userForRegistration.Email });

            if (existingUsers.Any())
            {
                throw new Exception("User with this email already exists!");
            }

        }




        public string RegistrEndInsert(UserForRegistrationDto userForRegistration)
        {
            byte[] passwordSalt = new byte[128 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }

            byte[] passwordHash = _authHelp.GetPasswordHash(userForRegistration.Password, passwordSalt);


            string token = _authHelp.CreateToken(userForRegistration.Email);

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

            if (_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters))
            {
                return token;
            }
            else
            {
                throw new Exception("Failed to register user.");
            }

        }

        public string CheckEmail(UserForLoginDto userForLogin)
        {
            string sqlForToken = @"SELECT [UserId], [TokenValue] FROM dbo.Tokens WHERE Email = @UserEmail";

            var token = _dapper.LoadDatatwoParam<Token>(sqlForToken, new { UserEmail = userForLogin.Email }).FirstOrDefault();

            if (token == null)
            {
                throw new Exception("Incorrect Email!");
            }
            return token.TokenValue;

        }
        public void CheckPassword(UserForLoginDto userForLogin)
        {
            string sqlForHashAndSalt = @"SELECT [PasswordHash], [PasswordSalt] FROM dbo.Tokens WHERE Email = @UserEmail";

            var userForConfirmation = _dapper.LoadDatatwoParam<UserForLoginConfirmationDto>(sqlForHashAndSalt, new { UserEmail = userForLogin.Email }).FirstOrDefault();

            if (userForConfirmation == null)
            {
                throw new Exception("Incorrect Email!");
            }
            byte[] passwordHash = _authHelp.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

            if (!passwordHash.SequenceEqual(userForConfirmation.PasswordHash))
            {
                throw new Exception("Incorrect password!");
            }


        }


    }
}