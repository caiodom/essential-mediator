# EssentialMediator Benchmarks

The benchmark suite measures mediator dispatch overhead with [BenchmarkDotNet](https://benchmarkdotnet.org/).

## Scenarios

- Direct request handler invocation (baseline)
- `Mediator.Send` with no pipeline behaviors
- `Mediator.Send` with one pass-through pipeline behavior
- Direct notification handler invocation (baseline)
- `Mediator.Publish` with one notification handler

The suite uses a minimal `IServiceProvider` implementation so the results focus on EssentialMediator dispatch rather than a specific dependency injection container.

## Run locally

Run benchmarks in Release mode from the repository root:

```bash
dotnet run -c Release --project benchmarks/EssentialMediator.Benchmarks/EssentialMediator.Benchmarks.csproj
```

Filter to one benchmark class when iterating:

```bash
dotnet run -c Release --project benchmarks/EssentialMediator.Benchmarks/EssentialMediator.Benchmarks.csproj -- --filter '*MediatorSendBenchmarks*'
```

## Interpreting results

- Compare results produced on the same machine and runtime.
- Prefer multiple benchmark runs before drawing conclusions.
- Do not treat GitHub-hosted runner timings as stable performance evidence.
- Allocation results from `MemoryDiagnoser` are as important as elapsed time for dispatcher changes.
- Update performance claims in public documentation only when they are backed by reproducible benchmark output.
