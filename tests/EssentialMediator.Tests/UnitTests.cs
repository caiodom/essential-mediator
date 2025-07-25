using EssentialMediator.Abstractions.Mediation;

namespace EssentialMediator.Tests;

public class UnitTests
{
    [Fact]
    public void Unit_Value_ShouldBeConsistent()
    {
        // Arrange & Act
        var unit1 = Unit.Value;
        var unit2 = Unit.Value;

        // Assert
        Assert.Equal(unit1, unit2);
    }

    [Fact]
    public void Unit_Equals_WithAnotherUnit_ShouldReturnTrue()
    {
        // Arrange
        var unit1 = new Unit();
        var unit2 = new Unit();

        // Act & Assert
        Assert.True(unit1.Equals(unit2));
        Assert.True(unit2.Equals(unit1));
    }

    [Fact]
    public void Unit_Equals_WithObject_ShouldReturnCorrectResult()
    {
        // Arrange
        var unit = new Unit();
        object unitObj = new Unit();
        object notUnit = "not a unit";

        // Act & Assert
        Assert.True(unit.Equals(unitObj));
        Assert.False(unit.Equals(notUnit));
        Assert.False(unit.Equals(null));
    }

    [Fact]
    public void Unit_GetHashCode_ShouldBeZero()
    {
        // Arrange
        var unit = new Unit();

        // Act & Assert
        Assert.Equal(0, unit.GetHashCode());
    }

    [Fact]
    public void Unit_ToString_ShouldReturnEmptyParentheses()
    {
        // Arrange
        var unit = new Unit();

        // Act & Assert
        Assert.Equal("()", unit.ToString());
    }

    [Fact]
    public async Task Unit_ImplicitConversion_ToValueTask_ShouldWork()
    {
        // Arrange
        var unit = Unit.Value;

        // Act
        ValueTask<Unit> valueTask = unit;

        // Assert
        Assert.True(valueTask.IsCompleted);
        Assert.Equal(unit, await valueTask);
    }

    [Fact]
    public async Task Unit_ImplicitConversion_ToTask_ShouldWork()
    {
        // Arrange
        var unit = Unit.Value;

        // Act
        Task<Unit> task = unit;

        // Assert
        Assert.True(task.IsCompleted);
        Assert.Equal(unit, await task);
    }

    [Fact]
    public void Unit_EqualityComparison_ShouldWork()
    {
        // Arrange
        var unit1 = new Unit();
        var unit2 = new Unit();

        // Act & Assert - Using Equals instead of operators
        Assert.True(unit1.Equals(unit2));
        Assert.True(unit2.Equals(unit1));
    }

    [Fact]
    public void Unit_DefaultValue_ShouldEqualValue()
    {
        // Arrange
        var defaultUnit = default(Unit);

        // Act & Assert
        Assert.Equal(Unit.Value, defaultUnit);
    }
}
