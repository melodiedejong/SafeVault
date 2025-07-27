namespace SafeVault.Core.Tests.SharedData
{
    using System.Collections.Generic;

    public static class TestData
    {
        public static IEnumerable<object[]> ValidInputCases =>
            new List<object[]>
            {
            new object[] { "Alice123", "", true },
            new object[] { "Bob!$#", "!$#", true },
            new object[] { "123456", "", true },
            new object[] { "abcDEF", "", true },
            new object[] { "abc123$", "$", true }
            };

        public static IEnumerable<object[]> InvalidInputCases =>
            new List<object[]>
            {
            new object[] { "Eve<script>", "", false },
            new object[] { "abc123$", "", false },
            new object[] { "abc 123", "", false },
            new object[] { "", "", false },
            new object[] { null, "", false }
            };
   public static IEnumerable<object[]> InputSanitizer_MaliciousCases =>
        new List<object[]>
        {
            new object[] { "<script>alert('XSS')</script>", "" },
            new object[] { "<img src='x' onerror='alert(1)'>", "" },
            new object[] { "'; DROP TABLE Users;--", "" },
            new object[] { "Robert'); EXEC xp_cmdshell('dir');--", "" },
            new object[] { "1 OR 1=1", "" }
        };

    public static IEnumerable<object[]> InputSanitizer_SafeCases =>
        new List<object[]>
        {
            new object[] { "Hello123", "Hello123" },
            new object[] { "Safe_Input!", "Safe_Input!" },
            new object[] { "Text with spaces", "Text with spaces" },
            new object[] { "Email@test.com", "Email@test.com" },
            new object[] { "1234567890", "1234567890" }
        };
    }
}