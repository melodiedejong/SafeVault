using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SafeVault.Core;

namespace SafeVault.Web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IInputSanitizer _sanitizer;

        public LoginModel(IAuthService authService, IInputSanitizer sanitizer)
        {
            _authService = authService;
            _sanitizer = sanitizer;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new LoginInputModel();

        public void OnGet()
        {
            // Display the login form
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Sanitize input
            Input.Username = _sanitizer.Sanitize(Input.Username);

            var token = await _authService.GenerateTokenAsync(
                Input.Username, Input.Password);

            if (string.IsNullOrWhiteSpace(token))
            {
                ModelState.AddModelError(string.Empty, 
                  "Invalid username or password.");
                return Page();
            }

            Response.Cookies.Append("JwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            return RedirectToPage("/Index");
        }
    }

    public class LoginInputModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;
    }
}
