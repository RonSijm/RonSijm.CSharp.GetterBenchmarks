namespace RonSijm.CSharp.GetterBenchmarks.Implementations
{
    public class StaticHttpMethodGetter : IHttpMethodGetter
    {
        private static readonly HttpMethod HttpMethodAccessor = HttpMethod.Get;

        // ReSharper disable once ReplaceAutoPropertyWithComputedProperty
        public HttpMethod HttpMethod { get; } = HttpMethodAccessor;
    }
}