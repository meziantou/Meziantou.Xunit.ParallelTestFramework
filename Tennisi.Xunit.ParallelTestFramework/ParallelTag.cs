using System.Security.Cryptography;
using System.Text;
using Xunit.Abstractions;

namespace Tennisi.Xunit;

public static class ParallelTag
{
    public static long ToPositiveInt64Hash(ITestMethod method, object[] args)
    {
        var code = method.TestClass.Class.Name + "." + method.Method.Name;

        if (args != null && args.Length > 0)
        {
            foreach (var arg in args)
            {
                code += "." + (arg?.ToString() ?? "null");
            }
        }

        return ToPositiveInt64Hash(code);
    }

    public static long ToPositiveInt64Hash(string input)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            long hashValue = BitConverter.ToInt64(hashBytes, 0);
            return hashValue & 0x7FFFFFFFFFFFFFFF;
        }
    }
}