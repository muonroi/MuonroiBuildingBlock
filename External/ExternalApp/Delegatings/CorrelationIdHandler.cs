namespace Muonroi.BuildingBlock.External.ExternalApp.Delegatings
{
    public class CorrelationIdHandler(MAuthenticateInfoContext authContext) : DelegatingHandler
    {
        private readonly MAuthenticateInfoContext _authContext = authContext ?? throw new ArgumentNullException(nameof(authContext));

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_authContext.CorrelationId))
            {
                request.Headers.Add(CustomHeader.CorrelationId, _authContext.CorrelationId);
            }

            if (!string.IsNullOrEmpty(_authContext.ApiKey))
            {
                request.Headers.Add(CustomHeader.ApiKey, _authContext.ApiKey);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}