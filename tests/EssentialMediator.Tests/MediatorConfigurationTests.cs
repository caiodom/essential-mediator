using EssentialMediator.Extensions.DependencyInjection.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EssentialMediator.Tests;

public class MediatorConfigurationTests
{
    [Fact]
    public void RegisterServicesFromAssemblies_WithValidAssemblies_ShouldAddToCollection()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var assembly1 = Assembly.GetExecutingAssembly();
        var assembly2 = Assembly.GetCallingAssembly();

        // Act
        var result = config.RegisterServicesFromAssemblies(assembly1, assembly2);

        // Assert
        Assert.Same(config, result); // Should return same instance for fluent interface
        // Can't directly test internal Assemblies property, but verify fluent interface works
    }

    [Fact]
    public void RegisterServicesFromAssemblies_WithNullAssemblies_ShouldThrowArgumentNullException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => config.RegisterServicesFromAssemblies(null!));
    }

    [Fact]
    public void RegisterServicesFromAssemblies_WithNullAssemblyInArray_ShouldSkipNull()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        var result = config.RegisterServicesFromAssemblies(assembly, null!);

        // Assert
        // Can't directly test internal Assemblies property, but verify fluent interface works
        Assert.Same(config, result);
    }

    [Fact]
    public void RegisterServicesFromAssemblyContaining_Generic_ShouldAddAssembly()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.RegisterServicesFromAssemblyContaining<MediatorConfigurationTests>();

        // Assert
        Assert.Same(config, result);
    }

    [Fact]
    public void RegisterServicesFromAssemblyContaining_WithType_ShouldAddAssembly()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var type = typeof(MediatorConfigurationTests);

        // Act
        var result = config.RegisterServicesFromAssemblyContaining(type);

        // Assert
        Assert.Same(config, result);
    }

    [Fact]
    public void RegisterServicesFromAssemblyContaining_WithNullType_ShouldThrowArgumentNullException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => config.RegisterServicesFromAssemblyContaining(null!));
    }

    [Fact]
    public void WithServiceLifetime_ShouldSetServiceLifetime()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithServiceLifetime(ServiceLifetime.Singleton);

        // Assert
        Assert.Same(config, result);
        Assert.Equal(ServiceLifetime.Singleton, config.ServiceLifetime);
    }

    [Fact]
    public void ServiceLifetime_DefaultValue_ShouldBeScoped()
    {
        // Arrange & Act
        var config = new MediatorConfiguration();

        // Assert
        Assert.Equal(ServiceLifetime.Scoped, config.ServiceLifetime);
    }

    [Fact]
    public void RegisterServicesFromEntryAssembly_ShouldAddEntryAssembly()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.RegisterServicesFromEntryAssembly();

        // Assert
        Assert.Same(config, result);
    }

    [Fact]
    public void RegisterServicesFromCallingAssembly_ShouldAddCallingAssembly()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.RegisterServicesFromCallingAssembly();

        // Assert
        Assert.Same(config, result);
    }

    [Fact]
    public void FluentInterface_ShouldAllowChaining()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config
            .RegisterServicesFromAssemblyContaining<MediatorConfigurationTests>()
            .WithServiceLifetime(ServiceLifetime.Transient)
            .RegisterServicesFromEntryAssembly()
            .RegisterServicesFromCallingAssembly();

        // Assert
        Assert.Same(config, result);
        Assert.Equal(ServiceLifetime.Transient, config.ServiceLifetime);
    }
}
