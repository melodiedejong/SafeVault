namespace SafeVault.Core
{
  public interface IAuthService
  {
    Task<string?> GenerateTokenAsync(string username, string password);
  }
}
