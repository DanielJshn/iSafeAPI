namespace apitest
{
    public partial class UserForLoginConfirmationDto
    {
        public byte[] PasswordHash {get; set;}
        public byte[] PasswordSalt {get; set;}

        public UserForLoginConfirmationDto()
        {
            if(PasswordHash == null)
            {
                PasswordHash = new byte[0];
            }
            if(PasswordSalt == null)
            {
                PasswordSalt = new byte[0];
            }
        }
    }
}