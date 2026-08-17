# Migrating to the built-in xUnit parallelization (`ParallelMode.All`)

xUnit.net v3 **4.0.0** adds full test parallelization to the core framework: the new
[`ParallelMode.All`](https://xunit.net/docs/running-tests-in-parallel#parallel-mode-all) runs every test in
parallel against every other test, regardless of test collections or shared context. That is exactly the problem
this package was created to solve, so if you are on xUnit v3 4.0.0 or later you should remove
`Meziantou.Xunit.ParallelTestFramework` / `Meziantou.Xunit.v3.ParallelTestFramework` and use the built-in feature
instead.

The built-in implementation goes further than this package did: it also supports opting out at the test method,
theory data source, and individual data row levels, and it no longer depends on theory pre-enumeration.

> [!NOTE]
> `ParallelMode.All` is a **v3 4.0+ core framework** feature. xUnit v2 only supports the `none` and `collections`
> modes and will not gain `all`. If you use `Meziantou.Xunit.ParallelTestFramework` (the v2 package), you must first
> [migrate the test project to xUnit v3](https://xunit.net/docs/getting-started/v3/migration) and then follow this
> guide.

## At a glance

| Before (this package)                                              | After (xUnit v3 4.0+)                                             |
| ------------------------------------------------------------------ | ----------------------------------------------------------------- |
| Install the package; everything runs in parallel                   | `[assembly: Parallelization(Mode = ParallelMode.All)]`            |
| `[DisableParallelization]` on a test class                         | `[TestClass(DisableParallelization = true)]`                      |
| `[DisableParallelization]` on a `[Fact]`/`[Theory]` method         | `[Fact(DisableParallelization = true)]` / `[Theory(...)]`         |
| `[DisableParallelization]` on a `[CollectionDefinition]`           | `[CollectionDefinition("Name", DisableParallelization = true)]`   |
| `[Collection("Name")]` (legacy implicit opt-out)                   | `[CollectionDefinition("Name", DisableParallelization = true)]`   |
| `DisableDiscoveryEnumeration = true` (legacy implicit opt-out)     | `[Theory(DisableParallelization = true)]`, or per row/data source |
| `[EnableParallelization]` on a collection or class                 | Nothing — `All` already parallelizes inside collections           |
| `preEnumerateTheories: true` required                              | Not required                                                      |
| Custom `DataAttribute` must return `SupportsDiscoveryEnumeration() => true` | Not required                                              |
| *(not supported)*                                                  | `[InlineData(42, DisableParallelization = true)]`                  |
| *(not supported)*                                                  | `new TheoryDataRow(42) { DisableParallelization = true }`          |

## Step 1 — Upgrade to xUnit v3 4.0.0 or later

```xml
<PackageReference Include="xunit.v3" Version="4.0.0" />
```

## Step 2 — Enable `ParallelMode.All`

Unlike this package, parallelization of tests within a class is **opt-in**: the default mode stays
`ParallelMode.Collections`. Pick whichever of the following mechanisms suits your project.

### Assembly attribute (recommended)

```c#
using Xunit.Sdk;   // ParallelMode
using Xunit.v3;    // ParallelizationAttribute

[assembly: Parallelization(Mode = ParallelMode.All)]
```

Note the two different namespaces: `ParallelizationAttribute` lives in `Xunit.v3`, while the `ParallelMode` enum
lives in `Xunit.Sdk`.

### `xunit.runner.json` / `testconfig.json`

```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "parallelMode": "all"
}
```

`parallelMode` accepts `"none"`, `"collections"`, and `"all"`, and is understood by both
[`xunit.runner.json`](https://xunit.net/docs/config-xunit-runner-json) and
[`testconfig.json`](https://xunit.net/docs/config-testconfig-json) for Microsoft Testing Platform. It is ignored by
runners and test frameworks older than 4.0, so it is safe to keep alongside `parallelizeTestCollections` if a single
config file is shared with older projects.

### Command line / MSBuild

```bash
dotnet run --project MyTests -- -parallelMode all
```

The MSBuild runner exposes the same setting as the `ParallelMode` property. Both are new to the 4.0 runners.

> [!IMPORTANT]
> VSTest `.runsettings` does not currently expose a `ParallelMode` setting — it only has the older
> `ParallelizeTestCollections` switch, which cannot express `all`. Use the assembly attribute or a config file
> instead.

## Step 3 — Remove this package

```bash
dotnet remove package Meziantou.Xunit.v3.ParallelTestFramework
```

The package registered its test framework through an MSBuild-injected
`[assembly: TestFramework(typeof(Meziantou.Xunit.v3.ParallelTestFramework))]` attribute, so removing the
`PackageReference` is enough — there is no leftover attribute to delete. If you had opted out of that injection with
`<IncludeMeziantouXunitParallelTestFramework>false</IncludeMeziantouXunitParallelTestFramework>` and registered the
framework yourself, remove your `[assembly: TestFramework]` attribute too.

## Step 4 — Replace the opt-out attributes

`DisableParallelizationAttribute` is replaced by a `DisableParallelization` property on the corresponding built-in
attribute. There is no single replacement attribute, because the built-in feature has more layers.

### Test class

```diff
-using Meziantou.Xunit.v3;
-
-[DisableParallelization]
+[TestClass(DisableParallelization = true)]
 public class SequentialTests
 {
     [Fact] public void Test1() => Thread.Sleep(2000);
     [Fact] public void Test2() => Thread.Sleep(2000);
 }
```

### Test method

```diff
 public class ParallelTests
 {
-    [Theory]
-    [DisableParallelization]
+    [Theory(DisableParallelization = true)]
     [InlineData(0), InlineData(1), InlineData(2)]
     public void Test4(int value) => Thread.Sleep(2000);
 }
```

### Test collection

```diff
-[CollectionDefinition("Sequential")]
-[DisableParallelization]
+[CollectionDefinition("Sequential", DisableParallelization = true)]
 public class SequentialCollection { }
```

If you relied on the legacy behavior where an explicit `[Collection("Name")]` attribute was itself enough to make a
class sequential, that no longer applies — you now need an explicit collection definition with
`DisableParallelization = true`, as above.

### New: data source and data row

These have no equivalent in this package:

```c#
public class TestClass1
{
    [Theory]
    [InlineData(0, DisableParallelization = true)] // runs non-parallel
    [InlineData(1)]                                // runs in parallel
    public void FromDataSource(int value) { }

    public static IEnumerable<TheoryDataRow<int>> Data =>
    [
        new(0) { DisableParallelization = true },   // runs non-parallel
        new(1),                                    // runs in parallel
    ];

    [Theory]
    [MemberData(nameof(Data))]
    public void FromDataRow(int value) { }
}
```

## Step 5 — Delete the workarounds you no longer need

### `EnableParallelizationAttribute`

`EnableParallelizationAttribute` exists only because this package kept collections sequential by default. Under
`ParallelMode.All` there is nothing to enable: test classes that share an `ICollectionFixture<T>` run in parallel
with each other, and so do the tests inside each class.

```diff
 [CollectionDefinition("MyFixture Collection")]
-[EnableParallelization]
 public class MyFixtureCollection : ICollectionFixture<MyFixture> { }
```

The usual caveat still applies: the fixture instance is shared, so it must be thread-safe.

### `preEnumerateTheories`

This package required [`preEnumerateTheories`](https://xunit.net/docs/config-xunit-runner-json) to be `true`,
because it could only parallelize theory rows that had been turned into individual test cases at discovery time.
That requirement is gone — the built-in scheduler parallelizes rows of delay-enumerated theories too. You can drop
the setting from `xunit.runner.json` unless you want it for other reasons (for example Visual Studio Code Lens).

### Custom `DataAttribute.SupportsDiscoveryEnumeration()`

For the same reason, a custom `DataAttribute` no longer has to return `true` from `SupportsDiscoveryEnumeration()`
for its rows to run in parallel. Keep the override only if you want the rows enumerated at discovery time.

```diff
 public sealed class CustomDataAttribute : DataAttribute
 {
     public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(MethodInfo testMethod, DisposalTracker disposalTracker)
         => new([new TheoryDataRow(1), new TheoryDataRow(2), new TheoryDataRow(3)]);
-
-    // No longer needed for parallel execution of theory data rows
-    public override bool SupportsDiscoveryEnumeration() => true;
 }
```

## Behavior differences to be aware of

### Opting out is assembly-wide, not scoped

This is the biggest semantic difference, and the one most likely to change your test timings.

With this package, `[DisableParallelization]` on a class meant *"the tests in this class run sequentially, but the
class still runs in parallel with the rest of the assembly."* With the built-in feature, a test that has opted out
of parallelism is guaranteed not to run in parallel against **any** other test in the assembly. Two different
opted-out classes therefore run one after the other, not side by side — and the same is true of two different
collections that both set `DisableParallelization = true`.

There is no built-in way to express "sequential within this class, parallel with everything else" under
`ParallelMode.All`. If a class was only opted out because a handful of tests touched shared mutable state, prefer
making that state per-test or guarding it with a lock and dropping the opt-out entirely; that keeps the
parallelism you migrated for. Reserve the opt-out attributes for tests that genuinely must run alone.

### Opting out is one-way

Once you opt out of parallelization at one layer, you cannot opt back in at a lower one. Collection ⇒ class ⇒
method ⇒ data source ⇒ data row: a disabled outer layer wins.

### `CollectionBehavior` properties are obsolete

If you configured threads or the scheduling algorithm, those properties moved to the new attribute and the old ones
are now obsolete and non-callable:

```diff
-[assembly: CollectionBehavior(MaxParallelThreads = 4, ParallelAlgorithm = ParallelAlgorithm.Aggressive)]
+[assembly: Parallelization(MaxThreads = 4, Algorithm = ParallelAlgorithm.Aggressive)]
```

`[assembly: CollectionBehavior(DisableTestParallelization = true)]` becomes
`[assembly: Parallelization(Mode = ParallelMode.None)]`. `CollectionBehavior` itself is still the way to set
`CollectionPerAssembly` / `CollectionPerClass`.

### Everything else that made your tests parallel-unsafe still applies

Moving from this package to `ParallelMode.All` does not change the degree of concurrency you get, so tests that
passed with the package will generally keep passing. Static state, fixtures, the current directory, environment
variables, and a shared database remain shared across concurrently running tests in the same class.

## Reference

- [Running Tests in Parallel](https://xunit.net/docs/running-tests-in-parallel) — parallel modes and every opt-out layer
- [Core Framework v3 4.0.0 release notes](https://xunit.net/releases/v3/4.0.0)
- [Config with `xunit.runner.json`](https://xunit.net/docs/config-xunit-runner-json)
- [Config with `testconfig.json`](https://xunit.net/docs/config-testconfig-json) (Microsoft Testing Platform)
- [Shared Context between Tests](https://xunit.net/docs/shared-context)
- [xunit/xunit#1986](https://github.com/xunit/xunit/issues/1986) — the original issue that this package was built from
