using SIMS.Interfaces;
using SIMS.Models;
using SIMS.SimsDbContext.Entities;
using BCrypt.Net;

namespace SIMS.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // Login - Verify password với hash
        public async Task<Users> LoginUser(string username, string password)
        {
            // Lấy user từ database
            var user = await _userRepository.GetUserByUsernameAsync(username);

            if (user == null)
            {
                return null;
            }

            // Kiểm tra password có phải BCrypt hash không
            bool isPasswordValid = false;

            if (IsBCryptHash(user.Password))
            {
                // Password đã hash - verify bằng BCrypt
                isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
            }
            else
            {
                // Password chưa hash - so sánh trực tiếp (cho user cũ)
                isPasswordValid = (password == user.Password);

                // Tự động hash lại password cho user cũ
                if (isPasswordValid)
                {
                    user.Password = HashPassword(password);
                    await _userRepository.UpdateUserAsync(user);
                }
            }

            if (!isPasswordValid)
            {
                return null;
            }

            return user;
        }

        // Register - Hash password trước khi lưu
        public async Task<(bool success, string message)> RegisterUser(RegisterViewModel model)
        {
            if (await _userRepository.UsernameExistsAsync(model.Username))
            {
                return (false, "Username already exists!");
            }

            if (await _userRepository.EmailExistsAsync(model.Email))
            {
                return (false, "Email already exists!");
            }

            var user = new Users
            {
                Username = model.Username,
                Email = model.Email,
                Password = HashPassword(model.Password), // Hash password ở đây
                FullName = model.FullName,
                PhoneNumber = model.Phone ?? "",
                Role = "User",
                Status = "Active",
                CreatedDate = DateTime.Now
            };

            var result = await _userRepository.RegisterUser(user);
            return result ? (true, "Registration successful!") : (false, "Error during registration!");
        }

        // Get all users
        public async Task<IEnumerable<Users>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        // Get user by ID
        public async Task<Users> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        // Get user by username
        public async Task<Users> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
                return null;

            return await _userRepository.GetUserByUsernameAsync(username);
        }

        // Create user - Hash password
        public async Task<bool> CreateUserAsync(Users user)
        {
            if (await _userRepository.UsernameExistsAsync(user.Username))
            {
                return false;
            }

            user.Password = HashPassword(user.Password); // Hash password
            user.CreatedDate = DateTime.Now;
            return await _userRepository.CreateUserAsync(user);
        }

        // Update user - Hash password nếu có thay đổi
        public async Task<bool> UpdateUserAsync(Users user)
        {
            // Lấy user cũ từ database
            var existingUser = await _userRepository.GetUserByIdAsync(user.Id);

            if (existingUser == null)
            {
                return false;
            }

            // Nếu password thay đổi thì hash lại
            if (!string.IsNullOrEmpty(user.Password) && user.Password != existingUser.Password)
            {
                user.Password = HashPassword(user.Password);
            }
            else
            {
                // Giữ nguyên password cũ nếu không thay đổi
                user.Password = existingUser.Password;
            }

            user.UpdatedDate = DateTime.Now;
            return await _userRepository.UpdateUserAsync(user);
        }

        // Delete user
        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userRepository.DeleteUserAsync(id);
        }

        // Hash password bằng BCrypt
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Verify password (có thể dùng public nếu cần)
        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        // Kiểm tra xem string có phải BCrypt hash không
        private bool IsBCryptHash(string password)
        {
            // BCrypt hash luôn bắt đầu bằng $2a$, $2b$, hoặc $2y$ và có độ dài ~60 ký tự
            return !string.IsNullOrEmpty(password) &&
                   password.Length >= 59 &&
                   (password.StartsWith("$2a$") ||
                    password.StartsWith("$2b$") ||
                    password.StartsWith("$2y$"));
        }
    }
}