using Xunit.Sdk;
using Xunit.v3;

#pragma warning disable IDE1006 // Naming Styles
namespace Meziantou.Xunit.v3;

internal sealed class ParallelTestExecutionContext : IDisposable
{
    private readonly SemaphoreSlim? _parallelSemaphore;
    private readonly MaxConcurrencySyncContext? _syncContext;

    public SynchronizationContext? SynchronizationContext => _syncContext;

    public static ParallelTestExecutionContext Default { get; } = new();

    public ParallelTestExecutionContext()
    {
    }

    public ParallelTestExecutionContext(XunitTestAssemblyRunnerContext context)
    {
        // When unlimited, we just launch everything and let the .NET Thread Pool sort it out
        if (context.MaxParallelThreads < 0)
            return;

        // For aggressive, we launch everything and let our sync context limit what's allowed to run
        if (context.ParallelAlgorithm == ParallelAlgorithm.Aggressive)
        {
            _syncContext = new MaxConcurrencySyncContext(context.MaxParallelThreads);
            SynchronizationContext.SetSynchronizationContext(_syncContext);
        }
        // For conversative, we use a semaphore to limit the number of launched tests, and ensure
        // that the .NET Thread Pool has enough threads based on the user's requested maximum
        else
        {
            _parallelSemaphore = new(initialCount: context.MaxParallelThreads);

            ThreadPool.GetMinThreads(out var minThreads, out var minIOPorts);
            var threadFloor = Math.Min(4, context.MaxParallelThreads);
            if (minThreads < threadFloor)
            {
                ThreadPool.SetMinThreads(threadFloor, minIOPorts);
            }
        }
    }

    public ValueTask WaitAsync(CancellationToken cancellationToken)
    {
        if (_parallelSemaphore is null)
            return new ValueTask();

        return new ValueTask(_parallelSemaphore.WaitAsync(cancellationToken));
    }

    public void Release()
    {
        _parallelSemaphore?.Release();
    }

    public void Dispose()
    {
        _parallelSemaphore?.Dispose();
        _syncContext?.Dispose();
    }
}
