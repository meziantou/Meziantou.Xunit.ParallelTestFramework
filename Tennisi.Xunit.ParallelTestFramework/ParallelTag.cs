using Xunit.Sdk;

namespace Tennisi.Xunit;

public readonly struct ParallelTag : IEquatable<ParallelTag>
{
    private readonly int _index = -1;

    public static ParallelTag? FromTestCase(object[]? constructorArguments, IXunitTestCase testCase, object[] args)
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

    public static void Inject(ref ParallelTag? tag, ref object[] args)
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

    public override string ToString()
    {
        return Value;
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