namespace DotchatServer.src.Application.Services;

public sealed class SnowflakeGenerator
{
    private const int SequenceBits = 12;
    private const int WorkerBits = 10;

    private const long SequenceMask = (1L << SequenceBits) - 1; // Max: 4095
    private const long WorkerMask = (1L << WorkerBits) - 1; // Max: 1023

    private readonly long _workerId;
    private long _lastTimestamp;
    private long _sequence = 0;

    private static readonly long _cacxEpoch = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
    private readonly object _lock = new();

    public SnowflakeGenerator(long workerId)
    {
        if (workerId is < 0 or > WorkerMask)
        {
            throw new ArgumentOutOfRangeException(nameof(workerId), $"Worker ID must be between 0 and {WorkerMask}.");
        }

        _workerId = workerId;
    }

    /// <summary>
    /// <see langword="return"/>s the the creation time of a given Snowflake ID as a <see cref="DateTimeOffset"/>. 
    /// </summary>
    /// <remarks>
    /// The creation time is derived from the timestamp portion of the Snowflake ID, which represents the number of milliseconds that have elapsed since the custom epoch (January 1, 2026). <br></br>
    /// This method allows you to determine when a particular Snowflake ID was generated based on its embedded timestamp.
    /// </remarks>
    /// <param name="id">The id that you want the <see cref="DateTimeOffset"/> extracted from</param>
    /// <returns></returns>
    public static DateTimeOffset GetCreationTime(long id)
    {
        long timestamp = (id >> 22) + _cacxEpoch;
        return DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
    }

    /// <summary>
    /// Generates the next unique 64-bit identifier in a thread-safe manner.
    /// </summary>
    /// <remarks>This method is safe for concurrent use by multiple threads. Identifiers are guaranteed to be
    /// unique and are typically used for distributed systems requiring unique IDs without central
    /// coordination.</remarks>
    /// <returns>A unique 64-bit integer representing the next identifier in the sequence.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the system clock moves backwards, resulting in a timestamp earlier than the last generated identifier.</exception>
    public long NextId()
    {
        lock (_lock)
        {
            long timestamp = GetCurrentTimestamp();

            if (timestamp < _lastTimestamp)
                throw new InvalidOperationException("Clock moved backwards.");

            if (timestamp == _lastTimestamp)
            {
                _sequence = (_sequence + 1) & SequenceMask;
                if (_sequence == 0)
                {
                    timestamp = WaitNextMillis(timestamp);
                }
            }
            else
            {
                _sequence = 0;
            }

            _lastTimestamp = timestamp;


            return ((timestamp - _cacxEpoch) << (WorkerBits + SequenceBits))
                   | (_workerId << SequenceBits)
                   | _sequence;
        }
    }

    private static long GetCurrentTimestamp()
        => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    private static long WaitNextMillis(long lastTimestamp)
    {
        SpinWait spin = new();
        long currentTimespan;

        do
        {
            spin.SpinOnce();
            currentTimespan = GetCurrentTimestamp();
        }
        while (currentTimespan <= lastTimestamp);

        return currentTimespan;
    }
}