namespace apitest
{
    public partial class UserForChangePassword
    {
        public string Password { get; set; }
        public string NewPassword {get; set;}
        public UserForChangePassword()
        {
            if (Password == null)
            {
                Password = "";
            }
            if (NewPassword == null)
            {
                NewPassword = "";
            }
        }
    }
}