namespace EssentialMediator.Abstractions.Mediation;

/// <summary>
/// Represents a successful request completion when no response value is required.
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>
    /// Gets the singleton-style value used to represent successful completion.
    /// </summary>
    public static readonly Unit Value = new();

    /// <summary>
    /// Determines whether this value is equal to another <see cref="Unit"/> value.
    /// </summary>
    /// <param name="other">The value to compare with.</param>
    /// <returns>Always <see langword="true"/> because all <see cref="Unit"/> values are equivalent.</returns>
    public bool Equals(Unit other) => true;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Unit;

    /// <inheritdoc />
    public override int GetHashCode() => 0;

    /// <inheritdoc />
    public override string ToString() => "()";

    /// <summary>
    /// Converts a <see cref="Unit"/> value to a completed <see cref="ValueTask{TResult}"/>.
    /// </summary>
    /// <param name="unit">The value to wrap.</param>
    public static implicit operator ValueTask<Unit>(Unit unit) => new(unit);

    /// <summary>
    /// Converts a <see cref="Unit"/> value to a completed <see cref="Task{TResult}"/>.
    /// </summary>
    /// <param name="unit">The value to wrap.</param>
    public static implicit operator Task<Unit>(Unit unit) => Task.FromResult(unit);
}
