namespace Meziantou.Xunit.ParallelTestFramework.Tests;

public class ConcurrencyFixture
{
    private int concurrency;

    public async Task<int> CheckConcurrencyAsync()
    {
        Interlocked.Increment(ref concurrency);
        await Task.Delay(TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);

        var overlap = concurrency;

        await Task.Delay(TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);
        Interlocked.Decrement(ref concurrency);

        return overlap;
    }
}