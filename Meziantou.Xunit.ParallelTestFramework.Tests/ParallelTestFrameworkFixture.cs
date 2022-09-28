namespace Meziantou.Xunit.ParallelTestFramework.Tests;

public class ParallelTestFrameworkFixture
{
    public Barrier FactBarrier { get; } = new Barrier(2);
    public Barrier TheoryBarrier { get; } = new Barrier(2);
}