namespace EssentialMediator.Abstractions.Mediation;

/// <summary>
/// Represents a void response
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    public static readonly Unit Value = new();

    public bool Equals(Unit other) => true;
    public override bool Equals(object? obj) => obj is Unit;
    public override int GetHashCode() => 0;
    public override string ToString() => "()";

    public static implicit operator ValueTask<Unit>(Unit unit) => new(unit);
    public static implicit operator Task<Unit>(Unit unit) => Task.FromResult(unit);
}
