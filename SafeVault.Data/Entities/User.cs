namespace SafeVault.Data.Entities
{
    public class User
    {
        public int    UserID   { get; set; }
        public string Username { get; set; } = default!;
        public string Email    { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string Role     { get; set; } = "User";

        // Account lockout properties
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }
    }
}

