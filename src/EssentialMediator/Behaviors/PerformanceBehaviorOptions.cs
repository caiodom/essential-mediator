namespace EssentialMediator.Behaviors;

/// <summary>
/// Configuration for the built-in performance monitoring behavior.
/// </summary>
public sealed class PerformanceBehaviorOptions
{
    /// <summary>
    /// Default threshold, in milliseconds, after which a request is considered slow.
    /// </summary>
    public const int DefaultSlowRequestThresholdMs = 500;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceBehaviorOptions"/> class.
    /// </summary>
    /// <param name="slowRequestThresholdMs">Threshold in milliseconds for considering a request slow.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the threshold is negative.</exception>
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
