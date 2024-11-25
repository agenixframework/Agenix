using System.Threading;

namespace Agenix.Core.Util;

public class AtomicLong(long initialValue = 0)
{
    private long _value = initialValue;

    public long Get()
    {
        return Interlocked.Read(ref _value);
    }

    public void Set(long newValue)
    {
        Interlocked.Exchange(ref _value, newValue);
    }

    public long IncrementAndGet()
    {
        return Interlocked.Increment(ref _value);
    }

    public long DecrementAndGet()
    {
        return Interlocked.Decrement(ref _value);
    }

    public long AddAndGet(long delta)
    {
        return Interlocked.Add(ref _value, delta);
    }

    public bool CompareAndSet(long expectedValue, long newValue)
    {
        return Interlocked.CompareExchange(ref _value, newValue, expectedValue) == expectedValue;
    }
}