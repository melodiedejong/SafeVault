using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SafeVault.Web.Pages;
using SafeVault.Core;
using SafeVault.Data.Entities;
using SafeVault.Data.Repositories;
using SafeVault.Web.Tests.Utilities; // assuming this is where TestHelpers lives

public class RegisterModelTests
{
    [Fact]
    public async Task OnPostAsync_WithValidInput_CreatesUserAndRedirects()
    {
        var sanitizerMock = TestHelpers.GetSanitizerMock();
        var repoMock = TestHelpers.GetRepoMock();
        var hasherMock = new Mock<IPasswordHasher<User>>();

        var model = new RegisterModel(sanitizerMock.Object, repoMock.Object, hasherMock.Object)
        {
            Input = new RegisterInputModel
            {
                Username = "TestUser",
                Email = "test@example.com",
                Password = "SecurePa$$123",
                Role = "Admin"
            }
        };

        var result = await model.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);
        repoMock.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Username == "TestUser" &&
            u.Email == "test@example.com" &&
            u.Role == "Admin" &&
            !string.IsNullOrEmpty(u.PasswordHash))), Times.Once);

        repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_WithInvalidModelState_ReturnsPageResult()
    {
        var sanitizerMock = TestHelpers.GetSanitizerMock();
        var repoMock = TestHelpers.GetRepoMock();
        var hasherMock = new Mock<IPasswordHasher<User>>();

        var model = new RegisterModel(sanitizerMock.Object, repoMock.Object, hasherMock.Object)
        {
            Input = new RegisterInputModel()
        };

        model.ModelState.AddModelError("Username", "Required");

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        repoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WithMissingPassword_ReturnsPageResult()
    {
        var sanitizerMock = TestHelpers.GetSanitizerMock();
        var repoMock = TestHelpers.GetRepoMock();
        var hasherMock = new Mock<IPasswordHasher<User>>();

        var model = new RegisterModel(sanitizerMock.Object, repoMock.Object, hasherMock.Object)
        {
            Input = new RegisterInputModel
            {
                Username = "validuser",
                Email = "valid@example.com",
                Password = ""
            }
        };

        model.ModelState.AddModelError("Password", "Required");

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        repoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WithInvalidEmailFormat_ReturnsPageResult()
    {
        var sanitizerMock = TestHelpers.GetSanitizerMock();
        var repoMock = TestHelpers.GetRepoMock();
        var hasherMock = new Mock<IPasswordHasher<User>>();

        var model = new RegisterModel(sanitizerMock.Object, repoMock.Object, hasherMock.Object)
        {
            Input = new RegisterInputModel
            {
                Username = "validuser",
                Email = "not-an-email",
                Password = "SecurePassword123"
            }
        };

        model.ModelState.AddModelError("Email", "Invalid format");

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        repoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_SanitizesInputBeforeCreatingUser()
    {
        var sanitizerMock = TestHelpers.GetSanitizerMock(s =>
            s switch
            {
                "rawuser" => "cleanuser",
                "raw@example.com" => "clean@example.com",
                _ => s
            });
        var repoMock = TestHelpers.GetRepoMock();
        var hasherMock = new Mock<IPasswordHasher<User>>();

        var model = new RegisterModel(sanitizerMock.Object, repoMock.Object, hasherMock.Object)
        {
            Input = new RegisterInputModel
            {
                Username = "rawuser",
                Email = "raw@example.com",
                Password = "SecurePassword123"
            }
        };

        var result = await model.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);

        sanitizerMock.Verify(s => s.Sanitize("rawuser"), Times.Once);
        sanitizerMock.Verify(s => s.Sanitize("raw@example.com"), Times.Once);

        repoMock.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Username == "cleanuser" && u.Email == "clean@example.com")), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_WithDuplicateUser_ReturnsPageWithConflictError()
    {
        var sanitizerMock = TestHelpers.GetSanitizerMock();
        var repoMock = TestHelpers.GetRepoMock();
        var hasherMock = new Mock<IPasswordHasher<User>>();

        // Simulate AddAsync throwing InvalidOperationException for duplicate user
        repoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                .ThrowsAsync(new InvalidOperationException("Username or email is already in use."));

        var model = new RegisterModel(sanitizerMock.Object, repoMock.Object, hasherMock.Object)
        {
            Input = new RegisterInputModel
            {
                Username = "existinguser",
                Email = "existing@example.com",
                Password = "SecurePassword123"
            }
        };

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.True(model.ModelState.ContainsKey(string.Empty));
        Assert.Contains("already in use", model.ModelState[string.Empty].Errors.First().ErrorMessage);

        repoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WithMissingRequiredFields_ReturnsPageWithValidationErrors()
    {
        var sanitizerMock = TestHelpers.GetSanitizerMock();
        var repoMock = TestHelpers.GetRepoMock();
        var hasherMock = new Mock<IPasswordHasher<User>>();

        var model = new RegisterModel(sanitizerMock.Object, repoMock.Object, hasherMock.Object)
        {
            Input = new RegisterInputModel
            {
                Username = "", // Missing
                Email = "",    // Missing
                Password = "", // Missing
                Role = ""      // Missing
            }
        };

        model.ModelState.AddModelError("Username", "Required");
        model.ModelState.AddModelError("Email", "Required");
        model.ModelState.AddModelError("Password", "Required");
        model.ModelState.AddModelError("Role", "Required");

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal(4, model.ModelState.ErrorCount);

        repoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WithEdgeCaseValidInputs_RegistersUserSuccessfully()
    {
        var sanitizerMock = TestHelpers.GetSanitizerMock();
        var repoMock = TestHelpers.GetRepoMock();
        var hasherMock = new Mock<IPasswordHasher<User>>();

        string maxLengthUsername = new string('a', 256); // Assuming 256 is the max valid length
        string maxLengthEmail = $"{new string('e', 64)}@{new string('d', 190)}.com"; // Constructed to max length
        string specialCharPassword = @"!@#$%^&*()_+-=[]{}|;':"",.<>?/`~Aa123";
        string specialCharRole = "Admin+Power@User"; // Allowable role format

        var model = new RegisterModel(sanitizerMock.Object, repoMock.Object, hasherMock.Object)
        {
            Input = new RegisterInputModel
            {
                Username = maxLengthUsername,
                Email = maxLengthEmail,
                Password = specialCharPassword,
                Role = specialCharRole
            }
        };

        model.ModelState.Clear(); // Assume valid inputs don't trigger model errors

        var result = await model.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);

        repoMock.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Username == maxLengthUsername &&
            u.Email == maxLengthEmail &&
            u.Role == specialCharRole)), Times.Once);

        repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_WithXSSPayload_SanitizesInputsBeforeSaving()
    {
        var xssPayload = "<script>alert('xss')</script>";
        var sanitized = "&lt;script&gt;alert('xss')&lt;/script&gt;"; // Assuming HTML encoding

        var sanitizerMock = new Mock<IInputSanitizer>();
        sanitizerMock.Setup(s => s.Sanitize(xssPayload)).Returns(sanitized);

        var repoMock = TestHelpers.GetRepoMock();
        var hasherMock = new Mock<IPasswordHasher<User>>();

        var model = new RegisterModel(sanitizerMock.Object, repoMock.Object, hasherMock.Object)
        {
            Input = new RegisterInputModel
            {
                Username = xssPayload,
                Email = xssPayload,
                Password = "StrongP@ssw0rd!",
                Role = "User"
            }
        };

        var result = await model.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);

        repoMock.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Username == sanitized &&
            u.Email == sanitized)), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_WithSQLInjectionPayload_SanitizesInputsBeforeSaving()
    {
        var sqlPayload = "'; DROP TABLE Users; --";
        var sanitized = "&#39;; DROP TABLE Users; --"; // Typical HTML entity for `'`

        var sanitizerMock = new Mock<IInputSanitizer>();
        sanitizerMock.Setup(s => s.Sanitize(sqlPayload)).Returns(sanitized);

        var repoMock = TestHelpers.GetRepoMock();
        var hasherMock = new Mock<IPasswordHasher<User>>();

        var model = new RegisterModel(sanitizerMock.Object, repoMock.Object, hasherMock.Object)
        {
            Input = new RegisterInputModel
            {
                Username = sqlPayload,
                Email = sqlPayload,
                Password = "StrongP@ssw0rd!",
                Role = "User"
            }
        };

        var result = await model.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);

        repoMock.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Username == sanitized &&
            u.Email == sanitized)), Times.Once);
    }



}
