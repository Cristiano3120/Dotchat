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

    private readonly long _cacxEpoch = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
    private readonly object _lock = new();
    public SnowflakeGenerator(long workerId)
    {
        if (workerId is < 0 or > WorkerMask)
        {
            throw new ArgumentOutOfRangeException(nameof(workerId), $"Worker ID must be between 0 and {WorkerMask}.");
        }

        _workerId = workerId;
    }

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