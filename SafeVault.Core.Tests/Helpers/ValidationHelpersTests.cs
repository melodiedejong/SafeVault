using Xunit;
using SafeVault.Core.Tests.SharedData;

namespace SafeVault.Core.Tests.Helpers
{
public class ValidationHelpersTests
{
    [Theory]
    [MemberData(nameof(TestData.ValidInputCases), MemberType = typeof(TestData))]
    public void IsValidInput_ValidInputs_ReturnsTrue(string input, string allowedSpecial, bool expected)
    {
        Assert.Equal(expected, ValidationHelpers.IsValidInput(input, allowedSpecial));
    }

    [Theory]
    [MemberData(nameof(TestData.InvalidInputCases), MemberType = typeof(TestData))]
    public void IsValidInput_InvalidInputs_ReturnsFalse(string input, string allowedSpecial, bool expected)
    {
        Assert.Equal(expected, ValidationHelpers.IsValidInput(input, allowedSpecial));
    }
}
}

