namespace RonSijm.CSharp.GetterBenchmarks.Implementations
{
    public class LambdaHttpMethodGetter : IHttpMethodGetter
    {
        public HttpMethod HttpMethod => HttpMethod.Get;
    }
}