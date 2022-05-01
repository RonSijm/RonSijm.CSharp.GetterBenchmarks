namespace RonSijm.CSharp.GetterBenchmarks.Implementations
{
    public class HttpMethodGetter : IHttpMethodGetter
    {
        public HttpMethod HttpMethod { get; } = HttpMethod.Get;
    }
}
