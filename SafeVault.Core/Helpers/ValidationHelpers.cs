public static class ValidationHelpers
{
  public static bool IsValidInput(string input, string allowedSpecial = "") =>
    !string.IsNullOrEmpty(input)
    && input.All(c => char.IsLetterOrDigit(c) || allowedSpecial.Contains(c));
}
