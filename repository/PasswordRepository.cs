
using api.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace apitest
{
    public class PasswordRepository
    {
        private readonly Datadapper _dapper;


        public PasswordRepository(Datadapper datadapper)
        {
            _dapper = datadapper;
        }

        public DateTime TestConnection()
        {
            return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
        }


        public List<Password> getAllPasswords(int userId)
        {
            string sql = @"SELECT * FROM dbo.[Passwords] WHERE UserId = @userId";
            IEnumerable<Password> passwords = _dapper.LoadDatatwoParam<Password>(sql, new { userId });

            List<Password> resultPasswords = new List<Password>();

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

            foreach (var additionalField in userInput.additionalFields)
            {
                string additionalFieldSql = @"
                INSERT INTO dbo.AdditionalFields (passwordId, title, [value])  
                VALUES (@passwordId, @title, @value)";

                var fieldParameters = new
                {
                    passwordId = id,
                    title = additionalField.title,
                    value = additionalField.value
                };

                if (!_dapper.ExecuteSQL(additionalFieldSql, fieldParameters))
                {
                    throw new Exception("Failed to add AdditionalField");
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

            foreach (var additionalField in userInput.additionalFields)
            {
                if (string.IsNullOrEmpty(additionalField.title) && string.IsNullOrEmpty(additionalField.value))
                {
                    additionalField.title = "";
                    additionalField.value = "";
                }
            }

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


        public void DeleteUser(int id)
        {
            string sqlUser = "DELETE FROM dbo.Tokens WHERE UserId = @id";

            if (!_dapper.ExecuteSQL(sqlUser, new { id }))
            {
                throw new Exception("Failed to delete User");
            }
        }




    }


}