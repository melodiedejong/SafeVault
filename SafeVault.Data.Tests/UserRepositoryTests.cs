using Xunit;
using SafeVault.Data.Repositories;
using SafeVault.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using SafeVault.Data;

public class UserRepositoryTests
{
    private SafeVaultContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SafeVaultContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SafeVaultContext(options);
    }

    [Fact]
    public async Task AddAsync_AddsUser_WhenUnique()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);
        var user = new User { Username = "unique_user", Email = "unique@example.com", PasswordHash = "hash123" };

        await repo.AddAsync(user);
        await repo.SaveChangesAsync();

        var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "unique_user");
        Assert.NotNull(savedUser);
        Assert.Equal("unique@example.com", savedUser?.Email);
    }

    [Fact]
    public async Task AddAsync_Throws_WhenUsernameExists()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);
        var user1 = new User { Username = "duplicate", Email = "first@example.com", PasswordHash = "hash123" };
        var user2 = new User { Username = "duplicate", Email = "second@example.com", PasswordHash = "hash456" };

        await repo.AddAsync(user1);
        await repo.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.AddAsync(user2));
    }

    [Fact]
    public async Task AddAsync_Throws_WhenEmailExists()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);
        var user1 = new User { Username = "user1", Email = "duplicate@example.com", PasswordHash = "hash123" };
        var user2 = new User { Username = "user2", Email = "duplicate@example.com", PasswordHash = "hash456" };

        await repo.AddAsync(user1);
        await repo.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.AddAsync(user2));
    }

    [Fact]
    public async Task GetByUsernameAsync_ReturnsCorrectUser()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);
        var user = new User { Username = "testuser", Email = "test@example.com", PasswordHash = "hash123" };

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var result = await repo.GetByUsernameAsync("testuser");
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result?.Email);
    }

    [Fact]
    public async Task GetByUsernameAsync_ReturnsNull_WhenNotFound()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        var result = await repo.GetByUsernameAsync("nonexistent");
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_Succeeds_WithMaxLengthUsernameAndEmail()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        // Assuming max length of 256 (common for username/email constraints)
        var maxUsername = new string('U', 256);
        var maxEmail = new string('e', 243) + "@x.com"; // total: 256

        var user = new User
        {
            Username = maxUsername,
            Email = maxEmail,
            PasswordHash = "hash123"
        };

        await repo.AddAsync(user);
        await repo.SaveChangesAsync();

        var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Username == maxUsername);
        Assert.NotNull(savedUser);
        Assert.Equal(maxEmail, savedUser?.Email);
    }

    [Fact]
    public async Task AddAsync_Succeeds_WithSpecialCharacters()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        var user = new User
        {
            Username = "user!@#$%^&*()_+=-",
            Email = "email+alias@domain.com",
            PasswordHash = "hash123"
        };

        await repo.AddAsync(user);
        await repo.SaveChangesAsync();

        var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "user!@#$%^&*()_+=-");
        Assert.NotNull(savedUser);
        Assert.Equal("email+alias@domain.com", savedUser?.Email);
    }

    [Theory]
    [InlineData(null, "valid@example.com")]
    [InlineData("validuser", null)]
    [InlineData("", "valid@example.com")]
    [InlineData("validuser", "")]
    public async Task AddAsync_Throws_WhenFieldsAreNullOrEmpty(string? username, string? email)
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        var user = new User
        {
            Username = username!,
            Email = email!,
            PasswordHash = "hash123"
        };

        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddAsync(user));
    }

    [Fact]
    public async Task GetByRoleAsync_WithValidRole_ReturnsExpectedUsers()
    {
        // Arrange
        var context = CreateContext();
        var repository = new UserRepository(context);

        var users = new[]
        {
        new User { Username = "alice", Email = "alice@example.com", PasswordHash = "hash1", Role = "Admin" },
        new User { Username = "bob", Email = "bob@example.com", PasswordHash = "hash2", Role = "User" },
        new User { Username = "carol", Email = "carol@example.com", PasswordHash = "hash3", Role = "Admin" }
    };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        // Act
        var admins = await repository.GetByRoleAsync("Admin");

        // Assert
        Assert.Equal(2, admins.Count);
        Assert.All(admins, u => Assert.Equal("Admin", u.Role));
    }

    [Fact]
    public async Task SaveChangesAsync_DoesNotThrow_WhenNoChanges()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        await repo.SaveChangesAsync(); // Should not throw
    }

    [Fact]
    public async Task GetByRoleAsync_ReturnsEmptyList_WhenRoleNotFound()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        var result = await repo.GetByRoleAsync("SuperAdmin");

        Assert.Empty(result);
    }
    [Fact]
    public async Task GetByRoleAsync_ReturnsEmptyList_WhenNoUsers()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        var result = await repo.GetByRoleAsync("User");

        Assert.Empty(result);
    }
    

}
