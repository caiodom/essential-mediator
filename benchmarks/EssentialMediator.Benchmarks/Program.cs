using BenchmarkDotNet.Running;
using EssentialMediator.Benchmarks;

BenchmarkSwitcher
    .FromAssembly(typeof(MediatorSendBenchmarks).Assembly)
    .Run(args);
