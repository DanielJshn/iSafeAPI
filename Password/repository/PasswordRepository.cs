

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace apitest
{
    public class PasswordRepository : IPasswordRepository
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


        public PasswordDto UpdatePassword(Guid id, PasswordDto userInput)
        {
            string updatePasswordQuery = @"
                        UPDATE dbo.Passwords 
                        SET password = @password, id =@id, organization =  @organization, title = @title , lastEdit = @lastEdit
                        WHERE id = @id";
            userInput.id = id;
            var passwordParameters = new
            {
                userInput.id,
                userInput.password,
                userInput.organization,
                userInput.title,
                userInput.lastEdit
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
                INSERT INTO dbo.AdditionalFields (passwordId, id, title, [value])  
                VALUES (@passwordId, @id, @title, @value)";

                var addFieldUUID = Guid.NewGuid();
                additionalField.id = addFieldUUID;

                var fieldParameters = new
                {
                    passwordId = userInput.id,
                    additionalField.id,
                    additionalField.title,
                    additionalField.value
                };

                if (!_dapper.ExecuteSQL(additionalFieldSql, fieldParameters))
                {
                    throw new Exception("Failed to add AdditionalField");
                }
            }
            var result = new PasswordDto
            {
                id = userInput.id,
                password = userInput.password,
                organization = userInput.organization,
                title = userInput.title,
                lastEdit = userInput.lastEdit,
                additionalFields = userInput.additionalFields
               .Where(field => !string.IsNullOrEmpty(field.title) || !string.IsNullOrEmpty(field.value))
               .ToList()
            };

            return result;

        }


        public PasswordDto PostPassword(int userId, PasswordDto passwordInput)
        {

            string passwordSql = @"
                INSERT INTO dbo.Passwords ([UserId], [id], [password], [organization], [title], [lastEdit])
                VALUES (@UserId, @Id, @Password, @Organization, @Title, @LastEdit);";

            var passwordUUID = Guid.NewGuid();
            passwordInput.id = passwordUUID;

            var passwordParameters = new
            {
                UserId = userId,
                passwordInput.id,
                passwordInput.password,
                passwordInput.organization,
                passwordInput.title,
                passwordInput.lastEdit
            };


            if (!_dapper.ExecuteSQL(passwordSql, passwordParameters))
            {
                throw new Exception("Failed to add Passwords");
            }

            foreach (var additionalField in passwordInput.additionalFields)
            {
                string additionalFieldSql = @"
                INSERT INTO dbo.AdditionalFields (passwordId, id, title, [value])  
                VALUES (@passwordId, @id, @title, @value)";

                var addFieldUUID = Guid.NewGuid();
                additionalField.id = addFieldUUID;

                var fieldParameters = new
                {
                    passwordId = passwordInput.id,
                    additionalField.id,
                    additionalField.title,
                    additionalField.value
                };

                if (!_dapper.ExecuteSQL(additionalFieldSql, fieldParameters))
                {
                    throw new Exception("Failed to add AdditionalField");
                }
            }

            foreach (var additionalField in passwordInput.additionalFields)
            {
                if (string.IsNullOrEmpty(additionalField.title) && string.IsNullOrEmpty(additionalField.value))
                {
                    additionalField.title = "";
                    additionalField.value = "";
                }
            }

            var result = new PasswordDto
            {
                id = passwordInput.id,
                password = passwordInput.password,
                organization = passwordInput.organization,
                title = passwordInput.title,
                lastEdit = passwordInput.lastEdit,
                additionalFields = passwordInput.additionalFields
                .Where(field => !string.IsNullOrEmpty(field.title) || !string.IsNullOrEmpty(field.value))
                .ToList()
            };

            return result;
        }


        public void  DeletePassword(Guid id)
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