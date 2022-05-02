using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Order;
using RonSijm.CSharp.GetterBenchmarks.Implementations;

namespace RonSijm.CSharp.GetterBenchmarks
{
    [MemoryDiagnoser]
    [TailCallDiagnoser]
    [EtwProfiler]
    [ConcurrencyVisualizerProfiler]
    [NativeMemoryProfiler]
    [ThreadingDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class BenchmarkPropertyAccessorsWithoutConstruction
    {
        private readonly HttpMethodGetter _httpMethodGetter = new();
        private readonly LambdaHttpMethodGetter _lambdaHttpMethodGetter = new();
        private readonly StaticHttpMethodGetter _staticHttpMethodGetter = new();
        private readonly StaticLambdaHttpMethodGetter _staticLambdaHttpMethodGetter = new();

        [Benchmark]
        public HttpMethod DefaultGetter()
        {
            return _httpMethodGetter.HttpMethod;
        }

        [Benchmark]
        public HttpMethod LambdaGetter()
        {
            return _lambdaHttpMethodGetter.HttpMethod;
        }

        [Benchmark]
        public HttpMethod DefaultStaticGetter()
        {
            return _staticHttpMethodGetter.HttpMethod;
        }

        [Benchmark]
        public HttpMethod LambdaStaticGetter()
        {
            return _staticLambdaHttpMethodGetter.HttpMethod;
        }
    }
}