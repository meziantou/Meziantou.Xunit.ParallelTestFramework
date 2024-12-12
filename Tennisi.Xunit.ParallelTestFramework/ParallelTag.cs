using System.Security.Cryptography;
using System.Text;
using Xunit.Sdk;

namespace Tennisi.Xunit;

/// <summary>
/// A readonly structure that serves as a <c>Xunit</c> fixture to provide unique but constant value for test fact or theory version,
/// facilitating parallel execution of tests while ensuring consistency in tagging.
/// </summary>
public readonly partial struct ParallelTag : IEquatable<ParallelTag>
{
    private readonly string _value;
    private readonly int _next;
    private readonly int _indexInConstructor;

    internal static ParallelTag? FromTestCase(object[]? constructorArguments, IXunitTestCase testCase)
    {
        if (constructorArguments == null)
            return null;

        for (var i = 0; i < constructorArguments.Length; i++)
        {
            if (constructorArguments[i] is ParallelTag)
            {
                var result = new ParallelTag(testCase.UniqueID, i, 0);
                return result;
            }
        }

        return null;
    }

    internal static void Inject(ref ParallelTag? tag, ref object[] args)
    {
        if (tag == null)
            throw new InvalidOperationException(nameof(ParallelTag));

        args[tag.Value._indexInConstructor] = tag;
    }

    private ParallelTag(string value, int indexInConstructor, int next)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or empty", nameof(value));

        _value = value;
        _indexInConstructor = indexInConstructor;
        _next = next;
    }

    public override string ToString() => $"{_value}_{_next}";

    /// <summary>
    /// Derives the next unique value based on the current unique tag.
    /// </summary>
    /// <returns>
    /// A new <see cref="ParallelTag"/> instance with a derived unique tag.
    /// </returns>
    public ParallelTag Next(int increment = 1)
    {
        return new ParallelTag(_value, 0, _next + increment);
    }

    /// <summary>
    /// Converts the unique tag to an integer representation.
    /// </summary>
    /// <returns>
    /// The integer value corresponding to the unique tag. 
    /// </returns>
    public int AsInteger()
    {
        return HashCode.Combine(_value, _next);
    }

    /// <summary>
    /// Converts the unique tag to a long representation.
    /// </summary>
    /// <returns>
    /// The long value corresponding to the unique tag. 
    /// </returns>
    public long AsLong()
    {
        long valueHash = 0;
        foreach (var c in _value)
        {
            valueHash = (valueHash * 31) + c;
        }
        return (valueHash ^ _next) & long.MaxValue;
    }

    /// <summary>
    /// Converts the unique tag to a GUID representation.
    /// </summary>
    /// <returns>
    /// The GUID value corresponding to the unique tag. 
    /// </returns>
    public Guid AsGuid()
    {
        var valueBytes = Encoding.UTF8.GetBytes(_value);
        var nextBytes = BitConverter.GetBytes(_next);
        var combinedBytes = new byte[valueBytes.Length + nextBytes.Length];
        Buffer.BlockCopy(valueBytes, 0, combinedBytes, 0, valueBytes.Length);
        Buffer.BlockCopy(nextBytes, 0, combinedBytes, valueBytes.Length, nextBytes.Length);
        var hashBytes = SHA256.HashData(combinedBytes);
        var guidBytes = new byte[16];
        Array.Copy(hashBytes, guidBytes, 16);
        return new Guid(guidBytes);
    }

    public bool Equals(ParallelTag other)
    {
        return _value == other._value && _next == other._next;
    }

    public override bool Equals(object obj)
    {
        return obj is ParallelTag other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_value, _next);
    }

    public static bool operator ==(ParallelTag left, ParallelTag right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ParallelTag left, ParallelTag right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ParallelTag"/> readonly struct from the specified key value.
    /// </summary>
    /// <param name="value">The key value used to initialize the <see cref="ParallelTag"/>. 
    /// This value should be sufficiently long and distinctive, as it is intended to derive other values.</param>
    /// <returns>A new instance of the <see cref="ParallelTag"/> readonly struct.</returns>
    public static ParallelTag FromValue(string value)
    {
        return new ParallelTag(value, 0, 0);
    }
}