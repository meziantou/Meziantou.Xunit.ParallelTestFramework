using Xunit;
using Xunit.Abstractions;

namespace Tennisi.Xunit.ParallelTestFramework.Tests;

public class RetryTest
{
    private const int RetryN = 3;

    public class CounterFixture
    {
        public int RunCount;
    }

    [RetryClass(retryCount: RetryN)]
    public class ClassFactSample : IClassFixture<CounterFixture>
    {
        private readonly CounterFixture _counter;

        public ClassFactSample(CounterFixture counter)
        {
            _counter = counter;

            counter.RunCount++;
        }

        [Fact]
        public void WillPassTheN_Time()
        {
            Assert.Equal(RetryN, _counter.RunCount);
        }
    }

    [RetryClass(retryCount: RetryN)]
    public class ClassPassTheorySample : IClassFixture<CounterFixture>
    {
        private readonly CounterFixture _counter;

        public ClassPassTheorySample(CounterFixture counter)
        {
            _counter = counter;

            counter.RunCount++;
        }

        [Theory]
        [InlineData(RetryN)]
        public void WillPassTheN_Time(int expectedCount)
        {
            var val = _counter.RunCount == expectedCount;
            Assert.True(val);
        }
    }

    [RetryClass(retryCount: RetryN)]
    public class ClassFailedTheorySample : IClassFixture<CounterFixture>
    {
        private readonly CounterFixture _counter;

        public ClassFailedTheorySample(CounterFixture counter)
        {
            _counter = counter;

            counter.RunCount++;
        }

        [Theory]
        [InlineData(RetryN - 1)]
        public void WillPassTheN_Time(int expectedCount)
        {
            var val = _counter.RunCount == expectedCount;
            Assert.False(val);
        }
    }

    public class FactSample : IClassFixture<CounterFixture>
    {
        private readonly CounterFixture _counter;

        public FactSample(CounterFixture counter)
        {
            _counter = counter;

            counter.RunCount++;
        }

        [Fact]
        public void WillFail()
        {
            Assert.NotEqual(RetryN, _counter.RunCount);
        }
    }

    public class RetryFactSample : IClassFixture<CounterFixture>
    {
        private readonly CounterFixture _counter;

        public RetryFactSample(CounterFixture counter)
        {
            _counter = counter;

            counter.RunCount++;
        }

        [RetryFact(retryCount: RetryN)]
        public void WillPassTheN_Time()
        {
            Assert.Equal(RetryN, _counter.RunCount);
        }
    }

    public class RetryPassTheorySample : IClassFixture<CounterFixture>
    {
        private readonly CounterFixture _counter;
        
        private readonly ITestOutputHelper _output;

        public RetryPassTheorySample(CounterFixture counter, ITestOutputHelper helper)
        {
            _counter = counter;
            _output = helper;

            counter.RunCount++;
        }

        [RetryTheory(retryCount: RetryN)]
        [InlineData(RetryN)]
        public void TheoryMethodShouldPass(int expectedCount)
        {
            _output.WriteLine(_counter.RunCount.ToString());
            Assert.True(_counter.RunCount >= expectedCount);
        }
    }

    public class RetryFailTheorySample : IClassFixture<CounterFixture>
    {
        private readonly CounterFixture _counter;

        public RetryFailTheorySample(CounterFixture counter)
        {
            _counter = counter;

            counter.RunCount++;
        }

        [RetryTheory(retryCount: RetryN)]
        [InlineData(RetryN -1)]
        public void TheoryMethodShouldFail(int expectedCount)
        {
            var val = _counter.RunCount == expectedCount;
            Assert.False(val);
        }
    }
}