using EssentialMediator.Extensions.DependencyInjection.Exceptions;
using System.Reflection;

namespace EssentialMediator.Tests;

public class MediatorRegistrationExceptionTests
{
    [Fact]
    public void Constructor_ShouldPreserveAssemblyAndLoaderExceptions()
    {
        var assembly = typeof(MediatorRegistrationExceptionTests).Assembly;
        var loaderException = new FileNotFoundException("Missing dependency", "Missing.Dependency.dll");
        var reflectionException = new ReflectionTypeLoadException(
            new Type?[] { typeof(string), null },
            new Exception[] { loaderException });

        var exception = new MediatorRegistrationException(assembly, reflectionException);

        Assert.Same(assembly, exception.Assembly);
        Assert.Single(exception.LoaderExceptions);
        Assert.Same(loaderException, exception.LoaderExceptions[0]);
        Assert.Contains(assembly.FullName!, exception.Message);
        Assert.Contains("Missing dependency", exception.Message);
        Assert.Same(reflectionException, exception.InnerException);
    }
}
