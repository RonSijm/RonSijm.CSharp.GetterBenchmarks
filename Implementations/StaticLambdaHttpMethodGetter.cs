namespace RonSijm.CSharp.GetterBenchmarks.Implementations
{
    public class StaticLambdaHttpMethodGetter : IHttpMethodGetter
    {
        private static readonly HttpMethod HttpMethodAccessor = HttpMethod.Get;

        public HttpMethod HttpMethod => HttpMethodAccessor;
    }
}