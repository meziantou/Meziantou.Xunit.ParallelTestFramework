using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using Xunit.Sdk;

namespace Tennisi.Xunit;

public readonly struct ParallelTag : IEquatable<ParallelTag>
{
    private readonly int _index = -1;

    internal static ParallelTag? FromTestCase(object[]? constructorArguments, IXunitTestCase testCase, object[] args)
    {
        if (constructorArguments == null)
            return null;
        for (var i = 0; i <= constructorArguments.Length - 1; i++)
        {
            var p = constructorArguments[i];
            if (p is not ParallelTag) continue;
            var result = new ParallelTag(testCase.UniqueID, i);
            return result;
        }

        return null;
    }

    internal static void Inject(ref ParallelTag? tag, ref object[] args)
    {
        if (tag == null)
            throw new InvalidOperationException(nameof(ParallelTag));
        args[tag.Value._index] = tag;
    }

    private string Value { get; } = "";

    private ParallelTag(string value, int indexInConstrcutor)
    {
        Value = value;
        _index = indexInConstrcutor;
    }

    internal static ParallelTag FromValue(string value)
    {
        var tag = new ParallelTag(value, 0);
        return tag;
    }

    public override string ToString()
    {
        return Value;
    }

    public int AsInteger()
    {
        var hashCode = 0;
        var length = Value.Length;

        for (var i = 0; i < length; i += 8)
        {
            var chunk = Value.Substring(i, Math.Min(8, length - i));
            var chunkValue = Convert.ToInt32(chunk, 16);
            hashCode ^= chunkValue;
        }

        hashCode %= int.MaxValue;
        return Math.Abs(hashCode);
    }

    public long AsLong()
    {
        long hashCode = 0;
        var length = Value.Length;
        for (var i = 0; i < length; i += 16)
        {
            var chunk = Value.Substring(i, Math.Min(16, length - i));
            var chunkValue = Convert.ToInt64(chunk, 16);
            hashCode ^= chunkValue;
        }
        hashCode = hashCode % long.MaxValue;
        return Math.Abs(hashCode);
    }
    
    public Guid AsGuid()
    {
        var hashBytes = new byte[16];
        var length = Value.Length;
        var bytePos = 0;
    
        for (var i = 0; i < length; i += 8)
        {
            var chunk = Value.Substring(i, Math.Min(8, length - i));
            var chunkValue = Convert.ToInt32(chunk, 16);
            var chunkBytes = BitConverter.GetBytes(chunkValue);
            for (var j = 0; j < chunkBytes.Length && bytePos < hashBytes.Length; j++, bytePos++)
            {
                hashBytes[bytePos] ^= chunkBytes[j];
            }
        }
        return new Guid(hashBytes);
    }

    public ParallelTag Next(int index = 1)
    {
        return FromValue(NextDerive(Value, index));
    }
    
    [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
    private static string NextDerive(string baseAddress, int index)
    {
        var indexPos = baseAddress.LastIndexOf(':');
        var basePart = indexPos >= 0 ? baseAddress.Substring(0, indexPos) : baseAddress;
        var newAddress = $"{basePart}:{index}";
        var inputBytes = Encoding.UTF8.GetBytes(newAddress);
        return Convert.ToHexString(inputBytes).ToLowerInvariant();
    }
    
    public bool Equals(ParallelTag other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        return obj is ParallelTag other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(ParallelTag left, ParallelTag right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ParallelTag left, ParallelTag right)
    {
        return !(left == right);
    }
}