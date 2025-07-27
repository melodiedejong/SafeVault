namespace SafeVault.Core
{
  public interface IInputSanitizer
  {
    string? Sanitize(string input);
  }
}
