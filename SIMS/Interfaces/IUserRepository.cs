using SIMS.SimsDbContext.Entities;

namespace SIMS.Interfaces
{
    public interface IUserRepository
    {
        // Authentication
        Task<Users> LoginUser(string username, string password);
        Task<bool> RegisterUser(Users user);

        // CRUD Operations
        Task<IEnumerable<Users>> GetAllUsersAsync();
        Task<Users> GetUserByIdAsync(int id);
        Task<Users> GetUserByUsernameAsync(string username);
        Task<bool> CreateUserAsync(Users user);
        Task<bool> UpdateUserAsync(Users user);
        Task<bool> DeleteUserAsync(int id);

        // Validation
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
    }
}