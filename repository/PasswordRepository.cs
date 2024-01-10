
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
            string sql = @"SELECT * FROM dbo.[Passwords] WHERE UserId =" + userId;
            IEnumerable<Passwords> passwords = _dapper.LoadData<Passwords>(sql);

            List<Passwords> resultPasswords = new List<Passwords>();

            foreach (var password in passwords)
            {
                string sql2 = @"SELECT * FROM dbo.AdditionalFields WHERE passwordId =" + password.id;
                IEnumerable<AdditionalField> additionalFields = _dapper.LoadData<AdditionalField>(sql2);
                password.additionalFields = additionalFields.ToList();
                resultPasswords.Add(password);
            }
            return resultPasswords;
        }

        public void UpdatePassword(int id, PasswordDto userInput)
        {
            try
            {
                string updatePasswordQuery = @"
                        UPDATE dbo.Passwords 
                        SET password = @password, organization =  @organization, title = @title 
                        WHERE id = @id";

                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    SqlCommand command = new SqlCommand(updatePasswordQuery, conn);
                    command.Parameters.AddWithValue("@password", userInput.password ?? "");
                    command.Parameters.AddWithValue("@organization", userInput.organization ?? "");
                    command.Parameters.AddWithValue("@title", userInput.title ?? "");
                    command.Parameters.AddWithValue("@id", id);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected < 1)
                    {
                        throw new Exception("Failed to update Passwords");
                    }
                    conn.Close();
                }
            }
            catch
            {
                throw new Exception("Exception during password update");
            }


            string deleteAdditionalFieldQuery = @"
                    DELETE FROM dbo.AdditionalFields 
                    WHERE passwordId = @id";

            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                SqlCommand deleteCommand = new SqlCommand(deleteAdditionalFieldQuery, conn);
                deleteCommand.Parameters.AddWithValue("@id", id);

                int deletedRows = deleteCommand.ExecuteNonQuery();

                if (deletedRows < 0)
                {
                    throw new Exception("Failed to delete existing AdditionalField entries");
                }

                conn.Close();
            }

            foreach (var additionalField in userInput.additionalFields)
            {
                string insertAdditionalFieldQuery = @"
                        INSERT INTO dbo.AdditionalFields (passwordId, title, [value])  
                        VALUES (@passwordId, @title, @value)";

                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    SqlCommand insertCommand = new SqlCommand(insertAdditionalFieldQuery, conn);
                    insertCommand.Parameters.AddWithValue("@passwordId", id);
                    insertCommand.Parameters.AddWithValue("@title", additionalField.title ?? "");
                    insertCommand.Parameters.AddWithValue("@value", additionalField.value ?? "");

                    int insertedRows = insertCommand.ExecuteNonQuery();

                    if (insertedRows < 1)
                    {
                        throw new Exception("Failed to add AdditionalField");
                    }

                    conn.Close();
                }
            }
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
             string countQuery = "SELECT COUNT(*) FROM dbo.AdditionalFields WHERE passwordId = " + id.ToString();
            var count = _dapper.LoadData<int>(countQuery).FirstOrDefault();

            if (count > 0)
            {
                string sqlAdditional = @"DELETE from dbo.AdditionalFields WHERE passwordId= " + id.ToString();

                 _dapper.ExecuteSQL(sqlAdditional);
               
            };

            string sqlPassword = @"DELETE from dbo.Passwords WHERE id= " + id.ToString();

            if (!_dapper.ExecuteSQL(sqlPassword))
            {
              
                 throw new Exception ("Failed to delete Passwords");
            }
        }


    }

}