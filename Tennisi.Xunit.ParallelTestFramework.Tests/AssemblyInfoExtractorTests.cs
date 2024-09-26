using Xunit;

namespace Tennisi.Xunit.ParallelTestFramework.Tests;

public class AssemblyInfoExtractorTests
{
    [Theory]
    [InlineData("Тенниси.Xunit.ParallelTestFramework.Tests, Version=1.0.0.0", "Тенниси.Xunit.ParallelTestFramework.Tests, Version=1.0.0.0")]
    [InlineData("Тенниси,   Юнит,   ТестФреймворк, тесты, Version=1.0.0-beta7,  Culture=neutral", "Тенниси, Юнит, ТестФреймворк, тесты, Version=1.0.0-beta7")]
    [InlineData("Тенниси, Юнит, ТестФреймворк, тесты, Version=1.0.0.1, PublicKeyToken=abcdef1234567890", "Тенниси, Юнит, ТестФреймворк, тесты, Version=1.0.0.1")]
    [InlineData("\t\"Тенниси, Xunit, ParallelTestFramework, Tests\", Version=1.2.3-alpha, Culture=en-US", "Тенниси, Xunit, ParallelTestFramework, Tests, Version=1.2.3-alpha")]
    [InlineData("Тенниси.Xunit, Version=2.1.0.0, PublicKeyToken=7a5c1e8d5b4e6ef1", "Тенниси.Xunit, Version=2.1.0.0")]
    [InlineData("  Тенниси,   Юнит, ТестФреймворк, тесты, Version=2.0.0.0, Culture=ru-RU  ", "Тенниси, Юнит, ТестФреймворк, тесты, Version=2.0.0.0")]
    [InlineData("Тенниси.Xunit, Version=1.0.0.0, Culture=fr-FR, PublicKeyToken=12ab34cd56ef78gh", "Тенниси.Xunit, Version=1.0.0.0")]
    [InlineData("Тенниси, Юнит, ТестФреймворк, тесты, Version=1.0.0-beta,  PublicKeyToken=aabbccddeeff0011", "Тенниси, Юнит, ТестФреймворк, тесты, Version=1.0.0-beta")]
    [InlineData("Тенниси.Xunit,  Version= 2.1.0.0,  Culture=de-DE,   PublicKeyToken=null", "Тенниси.Xunit, Version=2.1.0.0")]
    [InlineData("Тенниси.Xunit.ParallelTestFramework.Tests, Version= 1.0.0.0\t, Culture=ja-JP", "Тенниси.Xunit.ParallelTestFramework.Tests, Version=1.0.0.0")]
    [InlineData("Тенниси.Xunit, Version=1.0.0-beta3, PublicKeyToken=abcdef", "Тенниси.Xunit, Version=1.0.0-beta3")]
    [InlineData("Тенниси, Юнит, ТестФреймворк, тесты, Version=3.0.0.0-beta1, Culture=ru-RU", "Тенниси, Юнит, ТестФреймворк, тесты, Version=3.0.0.0-beta1")]
    public void ExtractNameAndVersion_VariousAssemblyNames_ReturnsExpected(string input, string expected)
    {
        var result = AssemblyInfoExtractor.ExtractNameAndVersion(input);
        Assert.Equal(expected, result);
    }
}