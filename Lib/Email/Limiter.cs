namespace Lib.Email;

public interface ILimiter
{
    /// <summary>
    /// Ensure that limits and it right state and check if limit allow to send new email.
    /// </summary>
    /// <returns>true if limit allows, false otherwise</returns>
    bool IsLimitAllow();

    /// <summary>
    /// Increment limiter. Is called if email is sent successfully.
    /// </summary>
    void IncrementLimiter();

    /// <summary>
    /// Set used to limit. Called if email returned response that limit is reached.
    /// </summary>
    void ReachedLimit();
}

public class DayLimiter : ILimiter
{
    private readonly int dayLimit;
    private int dayUsed = 0;

    private long lastReset;

    public DayLimiter(int dayLimit)
    {
        this.dayLimit = dayLimit;
        lastReset = DateTime.Today.Ticks;
    }

    public void IncrementLimiter() => Interlocked.Increment(ref dayUsed);

    public bool IsLimitAllow()
    {
        DateTime now = DateTime.Now;

        if (now.Ticks - lastReset > TimeSpan.TicksPerDay)
        {
            Interlocked.Exchange(ref dayUsed, 0);
            Interlocked.Exchange(ref lastReset, now.Date.Ticks);
        }

        return dayUsed < dayLimit;
    }

    public void ReachedLimit()
    {
        Interlocked.Exchange(ref dayUsed, dayLimit);
    }
}