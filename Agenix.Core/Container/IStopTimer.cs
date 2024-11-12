namespace Agenix.Core.Container;

/// Interface for stopping a timer.
/// /
public interface IStopTimer
{
    /// Sends a signal to the timer to stop. If the timer happens to be executing its nested actions,
    /// then it will continue until all nested actions have completed.
    /// /
    void StopTimer();
}