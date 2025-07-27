using Moq;
using SafeVault.Core;
using SafeVault.Data.Entities;
using SafeVault.Data.Repositories;
using SafeVault.Web.Pages;
namespace SafeVault.Web.Tests.Utilities
{
    public static class TestHelpers
    {
        public static Mock<IInputSanitizer> GetSanitizerMock(Func<string, string>? overrideSanitize = null)
        {
            var mock = new Mock<IInputSanitizer>();
            mock.Setup(s => s.Sanitize(It.IsAny<string>()))
                .Returns<string>(s => overrideSanitize?.Invoke(s) ?? s);
            return mock;
        }

        public static Mock<IUserRepository> GetRepoMock()
        {
            var mock = new Mock<IUserRepository>();
            mock.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            mock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            return mock;
        }

        public static RegisterInputModel GetValidRegisterInput() => new()
        {
            Username = "validuser",
            Email = "valid@example.com",
            Password = "SecurePa$$123",
            Role = "User"
        };

        public static LoginInputModel GetValidLoginInput() => new()
        {
            Username = "valid@example.com",
            Password = "SecurePa$$123"
        };
    }
}