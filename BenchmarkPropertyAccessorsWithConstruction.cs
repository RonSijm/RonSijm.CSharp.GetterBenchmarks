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
    public class BenchmarkPropertyAccessorsWithConstruction
    {
        [Benchmark]
        public HttpMethod DefaultGetter()
        {
            var endpointWithGetter = new HttpMethodGetter();
            return endpointWithGetter.HttpMethod;
        }

        [Benchmark]
        public HttpMethod LambdaGetter()
        {
            var endpointWithLambdaGetter = new LambdaHttpMethodGetter();
            return endpointWithLambdaGetter.HttpMethod;
        }

        [Benchmark]
        public HttpMethod DefaultStaticGetter()
        {
            var endpointWithStaticGetter = new StaticHttpMethodGetter();
            return endpointWithStaticGetter.HttpMethod;
        }

        [Benchmark]
        public HttpMethod LambdaStaticGetter()
        {
            var endpointWithStaticLambdaGetter = new StaticLambdaHttpMethodGetter();
            return endpointWithStaticLambdaGetter.HttpMethod;
        }
    }
}