using EssentialMediator.Exceptions;
using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Mediation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EssentialMediator.Tests;

public class ExceptionTests
{
    [Fact]
    public void HandlerNotFoundException_ShouldHaveCorrectProperties()
    {
        // Arrange
        var requestType = typeof(string);

        // Act
        var exception = new HandlerNotFoundException(requestType);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Contains("No handler registered for request type 'String'", exception.Message);
    }

    [Fact]
    public void HandlerNotFoundException_WithInnerException_ShouldPreserveInnerException()
    {
        // Arrange
        var requestType = typeof(string);
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new HandlerNotFoundException(requestType, innerException);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Equal(innerException, exception.InnerException);
        Assert.Contains("No handler registered for request type 'String'", exception.Message);
    }

    [Fact]
    public void MultipleHandlersException_ShouldHaveCorrectProperties()
    {
        // Arrange
        var requestType = typeof(string);
        var handlerCount = 3;

        // Act
        var exception = new MultipleHandlersException(requestType, handlerCount);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Equal(handlerCount, exception.HandlerCount);
        Assert.Contains("Multiple handlers (3) found for request type 'String'", exception.Message);
    }

    [Fact]
    public void HandlerConfigurationException_ShouldHaveCorrectProperties()
    {
        // Arrange
        var handlerType = typeof(string);
        var message = "Configuration error";

        // Act
        var exception = new HandlerConfigurationException(handlerType, message);

        // Assert
        Assert.Equal(handlerType, exception.HandlerType);
        Assert.Contains("Handler configuration error for 'String': Configuration error", exception.Message);
    }

    [Fact]
    public void HandlerConfigurationException_WithInnerException_ShouldPreserveInnerException()
    {
        // Arrange
        var handlerType = typeof(string);
        var message = "Configuration error";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new HandlerConfigurationException(handlerType, message, innerException);

        // Assert
        Assert.Equal(handlerType, exception.HandlerType);
        Assert.Equal(innerException, exception.InnerException);
        Assert.Contains("Handler configuration error for 'String': Configuration error", exception.Message);
    }

    [Fact]
    public void MediatorException_ShouldBeBaseClass()
    {
        // Arrange & Act
        var handlerNotFoundException = new HandlerNotFoundException(typeof(string));
        var multipleHandlersException = new MultipleHandlersException(typeof(string), 2);
        var handlerConfigurationException = new HandlerConfigurationException(typeof(string), "error");

        // Assert
        Assert.IsAssignableFrom<MediatorException>(handlerNotFoundException);
        Assert.IsAssignableFrom<MediatorException>(multipleHandlersException);
        Assert.IsAssignableFrom<MediatorException>(handlerConfigurationException);
    }
}
