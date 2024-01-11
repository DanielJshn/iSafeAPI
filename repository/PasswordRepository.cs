
using api.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace apitest
{
    class PasswordRepository
    {
        Datadapper _dapper;
        HttpContext _context;
        IConfiguration _config;

        public PasswordRepository(Datadapper datadapper, HttpContext context, IConfiguration config)
        {
            _dapper = datadapper;
            _context = context;
            _config = config;
        }

        public DateTime TestConnection()
        {
            return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
        }

        public int getUserId()
        {
            string? accessToken = _context.Request.Headers["Authorization"];
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

        public List<Passwords> getAllPasswords(int userId)
        {
            string sql = @"SELECT * FROM dbo.[Passwords] WHERE UserId = @userId";
            IEnumerable<Passwords> passwords = _dapper.LoadDatatwoParam<Passwords>(sql, new { userId });

            List<Passwords> resultPasswords = new List<Passwords>();

            foreach (var password in passwords)
            {
                string sql2 = @"SELECT * FROM dbo.AdditionalFields WHERE passwordId = @passwordId";
                IEnumerable<AdditionalField> additionalFields = _dapper.LoadDatatwoParam<AdditionalField>(sql2, new { passwordId = password.id });
                password.additionalFields = additionalFields.ToList();
                resultPasswords.Add(password);
            }

            return resultPasswords;
        }


        public void UpdatePassword(int id, PasswordDto userInput)
        {
            string updatePasswordQuery = @"
                        UPDATE dbo.Passwords 
                        SET password = @password, organization =  @organization, title = @title 
                        WHERE id = @id";

            var passwordParameters = new
            {
                id = id,
                userInput.password,
                userInput.organization,
                userInput.title
            };

            if (!_dapper.ExecuteSQL(updatePasswordQuery, passwordParameters))
            {
                throw new Exception("Failed to update Passwords");
            }
            




            string countQuery = "SELECT COUNT(*) FROM dbo.AdditionalFields WHERE passwordId = @id";

            var count = _dapper.LoadDatatwoParam<int>(countQuery, new { id }).FirstOrDefault();

            if (count > 0)
            {
                string sqlAdditional = "DELETE FROM dbo.AdditionalFields WHERE passwordId = @id";

                _dapper.ExecuteSQL(sqlAdditional, new { id });
            }
            string getIdQuery = "SELECT TOP 1 id FROM dbo.Passwords ORDER BY id DESC";
            int insertedPasswordId = _dapper.LoadData<int>(getIdQuery).FirstOrDefault();
            foreach (var additionalField in userInput.additionalFields)
            {
                string additionalFieldSql = @"
                INSERT INTO dbo.AdditionalFields (passwordId, title, [value])  
                VALUES (@passwordId, @title, @value)";

                var fieldParameters = new
                {
                    passwordId = insertedPasswordId,
                    title = additionalField.title,
                    value = additionalField.value
                };

                if (!_dapper.ExecuteSQL(additionalFieldSql, fieldParameters))
                {
                    throw new Exception("Failed to add AdditionalField");
                }
            }



            // string deleteAdditionalFieldQuery = @"
            //         DELETE FROM dbo.AdditionalFields 
            //         WHERE passwordId = @id";
            // List<AdditionalField> additionalField1 = new List<AdditionalField>();

            // if (additionalField1 != null && additionalField1.Count > 0)
            // {
            //     foreach (var additionalField in additionalField1)
            //     {
            //         var additionalFieldparameter = new
            //         {
            //             id = id,
            //             passwordId = additionalField.passwordId
            //         };

            //         if (!_dapper.ExecuteSQL(deleteAdditionalFieldQuery, additionalFieldparameter))
            //         {
            //             throw new Exception("Failed to Delete add");
            //         }
            //     }
            // }
            // string getIdQuery = "SELECT TOP 1 id FROM dbo.Passwords ORDER BY id DESC";
            // int insertedPasswordId = _dapper.LoadData<int>(getIdQuery).FirstOrDefault();
            // foreach (var additionalField in userInput.additionalFields)
            // {
            //     string additionalFieldSql = @"
            //     INSERT INTO dbo.AdditionalFields (passwordId, title, [value])  
            //     VALUES (@passwordId, @title, @value)";

            //     var fieldParameters = new
            //     {
            //         passwordId = insertedPasswordId,
            //         title = additionalField.title,
            //         value = additionalField.value
            //     };

            //     if (!_dapper.ExecuteSQL(additionalFieldSql, fieldParameters))
            //     {
            //         throw new Exception("Failed to add AdditionalField");
            //     }
            // }

        }



        public PasswordDto PostPassword(int userId, PasswordDto userInput)
        {

            string passwordSql = @"INSERT INTO dbo.Passwords ([UserId], [password], [organization], [title])
                                    VALUES (@UserId, @Password, @Organization, @Title);";

            var passwordParameters = new
            {
                UserId = userId,
                userInput.password,
                userInput.organization,
                userInput.title
            };

            if (!_dapper.ExecuteSQL(passwordSql, passwordParameters))
            {
                throw new Exception("Failed to add Passwords");
            }

            string getIdQuery = "SELECT TOP 1 id FROM dbo.Passwords ORDER BY id DESC";
            int insertedPasswordId = _dapper.LoadData<int>(getIdQuery).FirstOrDefault();
            foreach (var additionalField in userInput.additionalFields)
            {
                string additionalFieldSql = @"
                INSERT INTO dbo.AdditionalFields (passwordId, title, [value])  
                VALUES (@passwordId, @title, @value)";

                var fieldParameters = new
                {
                    passwordId = insertedPasswordId,
                    title = additionalField.title,
                    value = additionalField.value
                };

                if (!_dapper.ExecuteSQL(additionalFieldSql, fieldParameters))
                {
                    throw new Exception("Failed to add AdditionalField");
                }
            }

            // Проверяем, если поля title и value пусты, заменяем их на пустую строку
            foreach (var additionalField in userInput.additionalFields)
            {
                if (string.IsNullOrEmpty(additionalField.title) && string.IsNullOrEmpty(additionalField.value))
                {
                    additionalField.title = ""; // Заменяем пустое поле title на пустую строку
                    additionalField.value = ""; // Заменяем пустое поле value на пустую строку
                }
            }

            // Создаем новый объект для возврата, убирая пустые additionalFields
            var result = new PasswordDto();


            result.password = userInput.password;
            result.organization = userInput.organization;
            result.title = userInput.title;
            result.additionalFields = userInput.additionalFields
                .Where(field => !string.IsNullOrEmpty(field.title) || !string.IsNullOrEmpty(field.value))
                .ToList();

            return result;


        }
        public void DeletePassword(int id)
        {
            string countQuery = "SELECT COUNT(*) FROM dbo.AdditionalFields WHERE passwordId = @id";

            var count = _dapper.LoadDatatwoParam<int>(countQuery, new { id }).FirstOrDefault();

            if (count > 0)
            {
                string sqlAdditional = "DELETE FROM dbo.AdditionalFields WHERE passwordId = @id";

                _dapper.ExecuteSQL(sqlAdditional, new { id });
            }



            string sqlPassword = "DELETE FROM dbo.Passwords WHERE id = @id";

            if (!_dapper.ExecuteSQL(sqlPassword, new { id }))
            {
                throw new Exception("Failed to delete Passwords");
            }
        }


    }

}