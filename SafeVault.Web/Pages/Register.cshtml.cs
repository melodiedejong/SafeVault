using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SafeVault.Core;
using SafeVault.Data.Entities;
using SafeVault.Data.Repositories;
using Microsoft.AspNetCore.Identity;

namespace SafeVault.Web.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IInputSanitizer _sanitizer;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _hasher;

        public RegisterModel(
            IInputSanitizer sanitizer,
             IUserRepository userRepository,
             IPasswordHasher<User> hasher)
        {
            _sanitizer = sanitizer;
            _userRepository = userRepository;
            _hasher = hasher;

            Input = new RegisterInputModel();
        }


        [BindProperty]
        public RegisterInputModel Input { get; set; }

        public void OnGet()  { }

    public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // 1) Sanitize inputs
            Input.Username = _sanitizer.Sanitize(Input.Username);
            Input.Email    = _sanitizer.Sanitize(Input.Email);


            // 2) Map to entity (PasswordHash left blank until Phase 2)
            var user = new User
            {
                Username     = Input.Username,
                Email        = Input.Email,
                //         PasswordHash = _hasher.HashPassword(null, Input.Password), // hashes the password
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.Password),

                Role         = Input.Role
            };

            // 3) Persist with parameterized queries
            try
            {
                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }

            return RedirectToPage("Success");
        }
    }
    public class RegisterInputModel
    {
        [Required, StringLength(100, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = "User"; // Default to "User"

    }
}
