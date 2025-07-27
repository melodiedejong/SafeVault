
---

```markdown
# 🛡️ SafeVault

SafeVault is a secure, modular web application designed to safeguard sensitive user data through robust authentication, input sanitization, and vulnerability-aware development practices. Powered by ASP.NET Core, EF Core, and secure coding techniques, SafeVault prioritizes safety, scalability, and clarity.

## 🔍 Project Structure

```bash
SAFEVAULT/
├── SafeVault.Core             # Core business logic and shared interfaces
├── SafeVault.Data             # Database context, models, and EF Core configurations
├── SafeVault.Web              # Razor Pages application and UI
├── *.Tests                    # Dedicated test suites for each module
└── SafeVault.sln             # Solution file
```

## 🧠 Features

- **JWT Authentication** with secure storage in HttpOnly cookies
- **ASP.NET Core Identity** integration for user and role management
- **Role-Based Access Control (RBAC)** for granular permission handling
- **Input Sanitization** to prevent XSS and SQL Injection
- **Password Hashing** using bcrypt
- **Comprehensive Test Coverage** including security-focused test cases

## 🔐 Security Highlights

- Enforced input validation across all forms and endpoints
- Secure token generation and validation using cryptographically sound techniques
- OWASP-guided development for protecting against common vulnerabilities
- Role authorization policies to restrict access based on user claims

## 🧪 Testing Strategy

Each component—Core, Data, Web—is backed by a corresponding test project:
- Unit tests for business logic and authentication
- Integration tests for EF Core operations
- Security tests simulating XSS, SQL injection, and RBAC bypass attempts

## 🚀 Getting Started

1. Clone the repository  
   ```bash
   git clone https://github.com/melodiedejong/SafeVault.git
   ```

2. Apply EF Core migrations  
   ```bash
   dotnet ef database update --project SafeVault.Data
   ```

3. Run the Web project  
   ```bash
   dotnet run --project SafeVault.Web
   ```

## 🛠️ Technologies

- ASP.NET Core
- Razor Pages
- Entity Framework Core
- ASP.NET Core Identity
- bcrypt
- xUnit

## 📝 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## 🤝 Acknowledgments

Thanks to Microsoft Copilot for AI-assisted code generation, vulnerability detection, and test automation that shaped SafeVault’s development.

