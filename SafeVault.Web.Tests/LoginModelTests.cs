using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

using SafeVault.Web.Pages;
using SafeVault.Core;
using SafeVault.Web.Tests.Utilities;

public class LoginModelTests
{
    [Fact]
    public async Task OnPostAsync_WithValidCredentials_SetsCookieAndRedirects()
    {
        var authServiceMock = new Mock<IAuthService>();
        var sanitizerMock = new Mock<IInputSanitizer>();

        authServiceMock.Setup(s => s.GenerateTokenAsync("validuser", "SecurePa$$123"))
                       .ReturnsAsync("valid-jwt-token");

        sanitizerMock.Setup(s => s.Sanitize("validuser"))
                     .Returns("validuser");

        var model = new LoginModel(authServiceMock.Object, sanitizerMock.Object)
        {
            Input = new LoginInputModel
            {
                Username = "validuser",
                Password = "SecurePa$$123"
            }
        };

        model.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await model.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);
        authServiceMock.Verify(s => s.GenerateTokenAsync("validuser", "SecurePa$$123"), Times.Once);
        sanitizerMock.Verify(s => s.Sanitize("validuser"), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_WithInvalidModelState_ReturnsPage()
    {
        var authServiceMock = new Mock<IAuthService>();
        var sanitizerMock = new Mock<IInputSanitizer>();

        var model = new LoginModel(authServiceMock.Object, sanitizerMock.Object)
        {
            Input = new LoginInputModel()
        };

        model.ModelState.AddModelError("Username", "Required");

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnPostAsync_WithInvalidCredentials_ReturnsPageWithError()
    {
        var authServiceMock = new Mock<IAuthService>();
        var sanitizerMock = new Mock<IInputSanitizer>();

        authServiceMock.Setup(s => s.GenerateTokenAsync("baduser", "wrongpw"))
                       .ReturnsAsync((string?)null);

        sanitizerMock.Setup(s => s.Sanitize("baduser"))
                     .Returns("baduser");

        var model = new LoginModel(authServiceMock.Object, sanitizerMock.Object)
        {
            Input = new LoginInputModel
            {
                Username = "baduser",
                Password = "wrongpw"
            }
        };

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.True(model.ModelState.ContainsKey(string.Empty));
    }

    [Fact]
    public async Task OnPostAsync_SetsSecureHttpOnlyCookie()
    {
        var authServiceMock = new Mock<IAuthService>();
        var sanitizerMock = new Mock<IInputSanitizer>();

        authServiceMock.Setup(s => s.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync("secure-jwt-token");

        sanitizerMock.Setup(s => s.Sanitize(It.IsAny<string>()))
                     .Returns((string s) => s);

        var responseCookiesMock = new Mock<IResponseCookies>();
        var responseMock = new Mock<HttpResponse>();
        responseMock.Setup(r => r.Cookies).Returns(responseCookiesMock.Object);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Response).Returns(responseMock.Object);

        var model = new LoginModel(authServiceMock.Object, sanitizerMock.Object)
        {
            Input = new LoginInputModel
            {
                Username = "user",
                Password = "pass"
            },
            PageContext = new PageContext
            {
                HttpContext = httpContextMock.Object
            }
        };

        await model.OnPostAsync();

        responseCookiesMock.Verify(c => c.Append(
            "JwtToken",
            "secure-jwt-token",
            It.Is<CookieOptions>(opts => opts.HttpOnly && opts.Secure)
        ), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_WithEmptyToken_ReturnsPageResult()
    {
        var authServiceMock = new Mock<IAuthService>();
        var sanitizerMock = new Mock<IInputSanitizer>();

        authServiceMock.Setup(s => s.GenerateTokenAsync("user", "pass"))
                       .ReturnsAsync(string.Empty);

        sanitizerMock.Setup(s => s.Sanitize("user"))
                     .Returns("user");

        var model = new LoginModel(authServiceMock.Object, sanitizerMock.Object)
        {
            Input = new LoginInputModel { Username = "user", Password = "pass" },
            PageContext = new PageContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await model.OnPostAsync();

        var pageResult = Assert.IsType<PageResult>(result);
        Assert.True(model.ModelState.ContainsKey(string.Empty));
    }

    [Fact]
    public async Task OnPostAsync_WithXssPayload_ReturnsPageWithError()
    {
        var xssPayload = "<script>alert('xss')</script>";

        var authServiceMock = new Mock<IAuthService>();
        var sanitizerMock = new Mock<IInputSanitizer>();

        authServiceMock.Setup(s => s.GenerateTokenAsync("clean-user", "SecurePa$$123"))
                       .ReturnsAsync((string?)null);

        sanitizerMock.Setup(s => s.Sanitize(xssPayload))
                     .Returns("clean-user");

        var model = new LoginModel(authServiceMock.Object, sanitizerMock.Object)
        {
            Input = new LoginInputModel
            {
                Username = xssPayload,
                Password = "SecurePa$$123"
            },
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.True(model.ModelState.ContainsKey(string.Empty));
        Assert.Equal("clean-user", model.Input.Username);
    }

    // Test Trims Leading/Trailing Whitespace from username
    [Fact]
    public async Task OnPostAsync_WithWhitespaceInUsername_TrimsBeforeTokenGeneration()
    {
        var rawUsername = "   secure_user   ";
        var trimmedUsername = "secure_user";

        var sanitizerMock = new Mock<IInputSanitizer>();
        sanitizerMock.Setup(s => s.Sanitize(rawUsername)).Returns(trimmedUsername);

        var authServiceMock = new Mock<IAuthService>();
        authServiceMock.Setup(a => a.GenerateTokenAsync(trimmedUsername, "StrongP@ssw0rd!"))
                       .ReturnsAsync("mocked_jwt_token");

        var httpContext = new DefaultHttpContext();
        var model = new LoginModel(authServiceMock.Object, sanitizerMock.Object)
        {
            Input = new LoginInputModel
            {
                Username = rawUsername,
                Password = "StrongP@ssw0rd!"
            },
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };

        model.ModelState.Clear(); // Simulate valid model

        var result = await model.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);

        sanitizerMock.Verify(s => s.Sanitize(rawUsername), Times.Once);
        authServiceMock.Verify(a => a.GenerateTokenAsync(trimmedUsername, "StrongP@ssw0rd!"), Times.Once);
    }


}
