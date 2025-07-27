using Xunit;
using Moq;
using SafeVault.Core;
public class AuthServiceTests
{
    [Fact]
    public async Task GenerateTokenAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(a => a.GenerateTokenAsync("validUser", "validPass"))
                       .ReturnsAsync("mocked-jwt-token");

        // Act
        var token = await mockAuthService.Object.GenerateTokenAsync("validUser", "validPass");

        // Assert
        Assert.NotNull(token);
        Assert.Equal("mocked-jwt-token", token);
    }

    [Fact]
    public async Task GenerateTokenAsync_ShouldReturnNull_WhenCredentialsAreInvalid()
    {
        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(a => a.GenerateTokenAsync("invalidUser", "wrongPass"))
                       .ReturnsAsync((string?)null);

        var token = await mockAuthService.Object.GenerateTokenAsync("invalidUser", "wrongPass");

        Assert.Null(token);
    }
}
