
using api.Controllers;
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

        public void updatePassword(int id, PasswordDto userInput)
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
            catch (Exception ex)
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


    }

}