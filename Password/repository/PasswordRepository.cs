

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace apitest
{
    public class PasswordRepository : IPasswordRepository
    {
        private readonly DatadapperAsync _dapper;

        public PasswordRepository(DatadapperAsync dapper)
        {
            _dapper = dapper;
        }

        public async Task<List<Password>> GetAllPasswords(int userId)
        {
            string sql = @"SELECT * FROM dbo.Passwords WHERE UserId = @userId";
            IEnumerable<Password> passwords = await _dapper.LoadDatatwoParamAsync<Password>(sql, new { userId });

            List<Password> resultPasswords = new List<Password>();

            foreach (var password in passwords)
            {
                string sql2 = @"SELECT * FROM dbo.AdditionalFields WHERE passwordId = @passwordId";
                IEnumerable<AdditionalField> additionalFields = await _dapper.LoadDatatwoParamAsync<AdditionalField>(sql2, new { passwordId = password.id });
                password.additionalFields = additionalFields.ToList();
                resultPasswords.Add(password);
            }

            return resultPasswords;
        }

        public async Task<PasswordDto> UpdatePassword(Guid id, PasswordDto userInput)
        {
            string updatePasswordQuery = @"
                UPDATE dbo.Passwords 
                SET password = @password, id = @id, organization = @organization, title = @title, lastEdit = @lastEdit
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
            var result = await _dapper.ExecuteSQLAsync(updatePasswordQuery, passwordParameters);
            if (!result)
            {
                throw new Exception("Failed to update Passwords");
            }

            string countQuery = "SELECT COUNT(*) FROM dbo.AdditionalFields WHERE passwordId = @id";
            var countResult = await _dapper.LoadDatatwoParamAsync<int>(countQuery, new { id });
            var count = countResult.FirstOrDefault();

            if (count > 0)
            {
                string sqlAdditional = "DELETE FROM dbo.AdditionalFields WHERE passwordId = @id";
                await _dapper.ExecuteSQLAsync(sqlAdditional, new { id });
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

                if (!await _dapper.ExecuteSQLAsync(additionalFieldSql, fieldParameters))
                {
                    throw new Exception("Failed to add AdditionalField");
                }
            }

            var resultDto = new PasswordDto
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

            return resultDto;
        }


        public async Task<PasswordDto> PostPassword(int userId, PasswordDto passwordInput)
        {
            string passwordSql = @"
        INSERT INTO dbo.Passwords (UserId, id, password, organization, title, lastEdit)
        VALUES (@UserId, @Id, @Password, @Organization, @Title, @LastEdit)";

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

            bool passwordInserted = await _dapper.ExecuteSQLAsync(passwordSql, passwordParameters);
            if (!passwordInserted)
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

                bool fieldInserted = await _dapper.ExecuteSQLAsync(additionalFieldSql, fieldParameters);
                if (!fieldInserted)
                {
                    throw new Exception("Failed to add AdditionalField");
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


        public async Task DeletePassword(Guid id)
        {
            string countQuery = "SELECT COUNT(*) FROM dbo.AdditionalFields WHERE passwordId = @id";
            var count = (await _dapper.LoadDatatwoParamAsync<int>(countQuery, new { id })).FirstOrDefault();

            if (count > 0)
            {
                string sqlAdditional = "DELETE FROM dbo.AdditionalFields WHERE passwordId = @id";
                await _dapper.ExecuteSQLAsync(sqlAdditional, new { id });
            }

            string sqlPassword = "DELETE FROM dbo.Passwords WHERE id = @id";

            bool passwordDeleted = await _dapper.ExecuteSQLAsync(sqlPassword, new { id });
            if (!passwordDeleted)
            {
                throw new Exception("Failed to delete Passwords");
            }
        }

    }
}