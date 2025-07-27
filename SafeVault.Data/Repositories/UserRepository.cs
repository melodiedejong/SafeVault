using Microsoft.EntityFrameworkCore;
using SafeVault.Data.Entities;

namespace SafeVault.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SafeVaultContext _context;

        public UserRepository(SafeVaultContext context)
        {
            _context = context;
        }

        // LINQ-to-Entities is automatically parameterized
        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(u => u.Username == username);
        }

        // Example of explicit parameterized raw SQL
        public async Task<User?> GetByUsernameRawSqlAsync(string username)
        {
            return await _context.Users
                .FromSqlInterpolated($"SELECT * FROM Users WHERE Username = {username}")
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetByRoleAsync(string role)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Role == role)
                .ToListAsync();
    }

        public async Task AddAsync(User user)
        {
            // Check for existing username
            var existingUserByUsername = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == user.Username);
            if (existingUserByUsername != null)
                throw new InvalidOperationException($"Username '{user.Username}' is already taken.");

            // Check for existing email
            var existingUserByEmail = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUserByEmail != null)
                throw new InvalidOperationException($"Email '{user.Email}' is already registered.");

            if (string.IsNullOrWhiteSpace(user.Username))
                throw new ArgumentNullException(nameof(user.Username));
            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ArgumentNullException(nameof(user.Email));

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                throw new ArgumentNullException(nameof(user.PasswordHash));
            if (string.IsNullOrWhiteSpace(user.Role))
                throw new ArgumentNullException(nameof(user.Role));
            // Add the new user
            await _context.Users.AddAsync(user);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
