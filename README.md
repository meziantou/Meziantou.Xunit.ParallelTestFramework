### Правила

### А. Пакет Tennisi.Xunit.ParallelTestFramework подключенный к MonoRepo вместо стандартного без каких либо настроек

### -A.1. Факты в классе-тесте выполняются параллельно.
### -A.2. Варианты энной теории выполняются последовательно.
### &nbsp;&nbsp;&nbsp; -A.2.a. Если установить xUnit.preEnumerateTheories в true варинты теории будут выполняться параллельно.
### -A.3. Установка коллекции на класс вернет поведение к стандартному xUnit т.е. последовательному выполнению в коллекции
### &nbsp;&nbsp;&nbsp; -A.3.a Важно избавится от коллекций где можно 

___

### Б. Используется настройка FullTestParallelization (cм. в OnlineClientStoreTests)

### -Б.1. Факты и варианты теорий выполняются параллельно. (Настройки xUnit переопределяются 'в уме')

### -Б.2. Доступно получение ParallelTag в конструктор класса-теста (cм. в OnlineClientStoreTests)

### &nbsp;&nbsp;&nbsp; -Б.2.a.Таг уникален для факта или версии теории.
  ```` Т.Е. КОНСТРУКТОР ПОРОЖДАЕТСЯ ДЛЯ КАЖДОГОГО ФАКТА ИЛИ ВЕРСИИ ТТЕОРИИ ````
### &nbsp;&nbsp;&nbsp; -Б.2.б.Таг имеет представления как string/long/int/guid. 

___

### Настройка DisableTestParallelization отношения к этой библиотеке не имеет.



Tennisi! By default, xUnit runs all test cases in a test class synchronously.
This package extends the default test framework to execute tests in parallel.

```shell
dotnet add package Tennisi.Xunit.ParallelTestFramework
```

````c#
//All tests are run in parallel
public class ParallelTests
{
    [Fact]
    public void Test1() => Thread.Sleep(2000);

    [Fact]
    public void Test2() => Thread.Sleep(2000);

    [Theory]
    [InlineData(0), InlineData(1), InlineData(2)]
    public void Test3(int value) => Thread.Sleep(2000);

    // This test runs in parallel with other tests
    // However, its test cases are run sequentially
    [Theory]
    [InlineData(0), InlineData(1), InlineData(2)]
    public void Test4(int value) => Thread.Sleep(2000);
}
````
Previous versions of this package relied on built in xUnit attributes instead of exposing a dedicated `DisableParallelization` attribute.
For backwards compatibility, parallelization can also be disabled by adding an explicit `Collection` attribute or `Theory` attribute with `DisableDiscoveryEnumeration` enabled.

````c#
// All tests are run in parallel
public class ParallelTests
{
    [Fact]
    public void Test1() => Thread.Sleep(2000);

    [Fact]
    public void Test2() => Thread.Sleep(2000);

    [Theory]
    [InlineData(0), InlineData(1), InlineData(2)]
    public void Test3(int value) => Thread.Sleep(2000);

    // This test runs in parallel with other tests
    // However, its test cases are run sequentially because of DisableDiscoveryEnumeration
    [Theory]
    [MemberData(nameof(GetData), DisableDiscoveryEnumeration = true)]
    public void Test4(int value) => Thread.Sleep(2000);

    public static TheoryData<int> GetData() =>  new() { { 0 }, { 1 } };
}

// This collection runs in parallel with other collections
// However, its tests cases are run sequentially because the Collection is explicit
[Collection("Sequential")]
public class SequentialTests
{
    [Fact]
    public void Test1() => Thread.Sleep(2000);

    [Fact]
    public void Test2() => Thread.Sleep(2000);
}
````