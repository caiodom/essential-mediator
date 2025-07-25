# EssentialMediator Tests

This directory contains all test projects for the EssentialMediator library, ensuring code quality, reliability, and correctness.

## Test Projects

### EssentialMediator.Tests

Comprehensive unit tests covering:

- **Core Mediator Functionality** - Request/response and notification patterns
- **Dependency Injection** - Service registration and resolution
- **Exception Handling** - Custom exceptions and error scenarios
- **Edge Cases** - Boundary conditions and error paths

See the [Tests README](EssentialMediator.Tests/README.md) for detailed information about the test structure and execution.

## Running All Tests

### Command Line
```bash
# From the repository root
dotnet test

# Or from the tests directory
cd tests
dotnet test
```

### Visual Studio
1. Open the solution file
2. Use Test Explorer to run all tests
3. View results and coverage information

### Test Output Example
```bash
Starting test execution, please wait...
A total of 45 tests ran in 2.3 seconds.
  - 45 passed
  - 0 failed
  - 0 skipped

Test Run Successful.
```

## Test Strategy

The test suite follows these principles:

- **Comprehensive Coverage** - All public APIs and critical paths
- **Fast Execution** - Unit tests that run quickly in CI/CD
- **Isolated Tests** - No dependencies between test methods
- **Clear Naming** - Test names that describe the scenario and expected outcome
- **Arrange-Act-Assert** - Consistent test structure

## Continuous Integration

Tests are automatically executed on:
- Pull requests
- Commits to main branch
- Release builds
- Scheduled runs

## Quality Gates

All tests must pass before:
- Merging pull requests
- Creating releases
- Publishing NuGet packages

## Adding New Tests

When contributing new features:

1. **Write tests first** - TDD approach recommended
2. **Cover all scenarios** - Happy path, error cases, edge cases
3. **Follow naming conventions** - Method_Scenario_ExpectedResult
4. **Update documentation** - Keep test documentation current
5. **Verify CI passes** - Ensure all automated checks succeed

## Test Dependencies

- **xUnit** - Primary test framework
- **Microsoft.Extensions.DependencyInjection** - DI testing
- **Microsoft.Extensions.Logging** - Logging verification
- **FluentAssertions** - Enhanced assertions (if used)
