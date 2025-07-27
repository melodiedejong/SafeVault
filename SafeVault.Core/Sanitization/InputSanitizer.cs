using System.Text.RegularExpressions;

namespace SafeVault.Core.Sanitization
{
  public class InputSanitizer : IInputSanitizer
  {
    // removes HTML tags and dangerous punctuation
    private static readonly Regex _cleaner = new(
      @"<.*?>|[^a-zA-Z0-9_@\\.\\-]", RegexOptions.Compiled);

public string? Sanitize(string input)
    => input == null
        ? null
        : string.IsNullOrWhiteSpace(input)
            ? ""
            : _cleaner.Replace(input, string.Empty).Trim();
  }
}
