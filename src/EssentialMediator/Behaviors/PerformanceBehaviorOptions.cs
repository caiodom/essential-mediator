namespace EssentialMediator.Behaviors;

/// <summary>
/// Configuration for the built-in performance monitoring behavior.
/// </summary>
public sealed class PerformanceBehaviorOptions
{
    public const int DefaultSlowRequestThresholdMs = 500;

    public PerformanceBehaviorOptions(int slowRequestThresholdMs = DefaultSlowRequestThresholdMs)
    {
        if (slowRequestThresholdMs < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(slowRequestThresholdMs),
                slowRequestThresholdMs,
                "Slow request threshold must be greater than or equal to zero.");
        }

        SlowRequestThresholdMs = slowRequestThresholdMs;
    }

    /// <summary>
    /// Gets the threshold, in milliseconds, after which a request is considered slow.
    /// </summary>
    public int SlowRequestThresholdMs { get; }
}
