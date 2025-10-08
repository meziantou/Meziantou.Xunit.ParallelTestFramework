using Xunit;

namespace Meziantou.Xunit.ParallelTestFramework.Tests;

[CollectionDefinition(nameof(ParallelTests))]
[EnableParallelization]
public class ParallelTestsCollection;
