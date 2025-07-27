using SafeVault.Data.Entities;

namespace SafeVault.Data.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task AddAsync(User user);
        Task SaveChangesAsync();
        Task<List<User>> GetByRoleAsync(string role);

    }
}
