
namespace apitest
{
    public interface IPasswordRepository
    {
        Task<List<Password>> GetAllPasswords(int userId);
        Task<PasswordDto> UpdatePassword(Guid id, PasswordDto userInput);
        Task<PasswordDto> PostPassword(int userId, PasswordDto passwordInput);
        Task DeletePassword(Guid id);
    }

}