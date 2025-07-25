# EssentialMediator.Tests

Unit tests for the EssentialMediator library, ensuring reliability and correctness of the mediator pattern implementation.

## Test Structure

This project contains comprehensive unit tests covering:

- **Mediator Core Functionality** - Request/Response and Notification patterns
- **Handler Registration** - Dependency injection and service resolution
- **Exception Handling** - Custom exceptions and error scenarios
- **Edge Cases** - Null handling, empty collections, and boundary conditions

## Test Categories

### MediatorTests.cs
- Request handling with responses
- Request handling without responses (commands)
- Service resolution and dependency injection
- Error scenarios and exception handling

### NotificationTests.cs
- Notification publishing to multiple handlers
- Handler execution order and parallelism
- Error handling in notification scenarios

## Running Tests

### Command Line
```bash
cd tests/EssentialMediator.Tests
dotnet test
```

### Visual Studio
Open the solution file and use Test Explorer to run all tests or specific test categories.

### Test Output
```bash
Starting test execution, please wait...
A total of X tests ran in Y seconds.
Test Run Successful.
```

## Test Dependencies

- **xUnit** - Test framework
- **Microsoft.Extensions.DependencyInjection** - DI container for testing
- **Microsoft.Extensions.Logging** - Logging infrastructure

## Coverage

The test suite aims for comprehensive coverage of:
- All public APIs
- Error conditions and edge cases
- Integration scenarios with dependency injection
- Performance characteristics

## Adding New Tests

When adding new features to EssentialMediator, ensure:

1. **Unit tests** for new functionality
2. **Integration tests** for DI scenarios
3. **Error handling tests** for exception paths
4. **Performance tests** for critical paths

## Test Conventions

- Test methods follow the pattern: `Method_Scenario_ExpectedResult`
- Use descriptive test names that explain the scenario
- Arrange-Act-Assert pattern for test structure
- Mock external dependencies when appropriate
