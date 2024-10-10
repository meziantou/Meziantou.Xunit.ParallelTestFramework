namespace Tennisi.Xunit;

public readonly partial struct ParallelTag
{
    internal const int MinTcpPort = 49152;
    internal const int MaxTcpPort = 65535;
    private static int _port = MinTcpPort-1;
    private static readonly object PortLock = new();
    
    /// <summary>
    /// Reserves a unique TCP port number within the predefined range (49152 - 65535).
    /// The reservation occurs globally while your project's testing process is ongoing. 
    /// Use this method if you need to test a specific network service in parallel.
    /// </summary>
    /// <returns>
    /// A unique port number that corresponds to the reserved tag, constrained to the predefined range.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the maximum number of ports has been reserved, exceeding <see cref="MaxTcpPort"/>.
    /// </exception>
    public int ReserveTcpPort()
    {
        lock (PortLock)
        {
            _port++;
            if (_port > MaxTcpPort)
                throw new InvalidOperationException($"Maximum number of ports are reserved: {MaxTcpPort}");
            return _port;
        }
    }
}