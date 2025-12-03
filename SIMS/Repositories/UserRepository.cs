using Microsoft.EntityFrameworkCore;
using SIMS.Interfaces;
using SIMS.SimsDbContext;
using SIMS.SimsDbContext.Entities;

namespace SIMS.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SimDbContext _context;

        public UserRepository(SimDbContext context)
        {
            _context = context;
        }

        // Authentication
        public async Task<Users> LoginUser(string username, string password)
        {
            return await _context.User
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
        }

        public async Task<bool> RegisterUser(Users user)
        {
            try
            {
                _context.User.Add(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // CRUD Operations
        public async Task<IEnumerable<Users>> GetAllUsersAsync()
        {
            return await _context.User
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();
        }

        public async Task<Users> GetUserByIdAsync(int id)
        {
            return await _context.User.FindAsync(id);
        }

        public async Task<Users> GetUserByUsernameAsync(string username)
        {
            return await _context.User
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> CreateUserAsync(Users user)
        {
            try
            {
                user.CreatedDate = DateTime.Now;
                _context.User.Add(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(Users user)
        {
            try
            {
                user.UpdatedDate = DateTime.Now;
                _context.User.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _context.User.FindAsync(id);
                if (user != null)
                {
                    _context.User.Remove(user);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Validation
        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.User
                .AnyAsync(u => u.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.User
                .AnyAsync(u => u.Email == email);
        }
    }
}