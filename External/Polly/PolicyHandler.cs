namespace Muonroi.BuildingBlock.External.Polly
{
    public class PolicyHandler(IAsyncPolicy<HttpResponseMessage> policy) : DelegatingHandler
    {
        private readonly IAsyncPolicy<HttpResponseMessage> _policy = policy;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await _policy.ExecuteAsync(() => base.SendAsync(request, cancellationToken));
        }
    }
}