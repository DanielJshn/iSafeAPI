namespace apitest
{
    public interface IAuthRepository
    {
        Task CheckUserAsync(UserForRegistrationDto userForRegistration);
        Task<string> RegistrEndInsertAsync(UserForRegistrationDto userForRegistration);
        Task<string> CheckEmailAsync(UserForLoginDto userForLogin);
        Task CheckPasswordAsync(UserForLoginDto userForLogin);
        Task<byte[]?> GetSaltForUserIdAsync(int userId);
        Task<byte[]?> GetHashForUserIdAsync(int userId);
        Task ChangeUserPasswordAsync(int userId, byte[] newPasswordHash);
        Task DeletePasswordDataAsync(List<Password> resultPasswords, int userId);
        Task DeleteUserAsync(int id);
    }

}