using Xunit;

namespace Tennisi.Xunit.ParallelTestFramework.Tests;

public class ReserveTcpPortTests
{
    [Fact]
    public void ItShouldReserveTcpPortsAndNotReserveDueToOverflow()
    {
        //arrange
        var range = Enumerable.Range(ParallelTag.MinTcpPort, ParallelTag.MaxTcpPort - ParallelTag.MinTcpPort + 1)
            .ToList();
        var ports = new List<int>();
        foreach (var port in range)
        {
            var ptag = ParallelTag.FromValue($"PORT:{port}");
            var tag = ptag.ReserveTcpPort();
            ports.Add(tag);
        }

        //assert
        Assert.True(range.SequenceEqual(ports));
        foreach (var tag in ports)
            Assert.InRange(tag, ParallelTag.MinTcpPort, ParallelTag.MaxTcpPort);

        try
        {
            var ptag = ParallelTag.FromValue($"PORT:{ParallelTag.MaxTcpPort + 1}");
            ptag.ReserveTcpPort();
            throw new InvalidOperationException("Test should fail");
        }
        catch (Exception e)
        {
            Assert.True(e is InvalidOperationException);
            Assert.True(e.Message == "Maximum number of ports are reserved: 65535");
        }
    }
}