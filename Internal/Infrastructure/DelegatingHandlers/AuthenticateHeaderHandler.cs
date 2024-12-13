﻿namespace Muonroi.BuildingBlock.Internal.Infrastructure.DelegatingHandlers
{
    internal class AuthenticateHeaderHandler : DelegatingHandler
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        private readonly MAuthenticateInfoContext _authContext;

        public IConfiguration _configuration;

        public AuthenticateHeaderHandler(ILogger<AuthenticateHeaderHandler> logger, MAuthenticateInfoContext authContext, IConfiguration configuration)
        {
            _logger = logger;
            _authContext = authContext;
            InnerHandler = new HttpClientHandler();
            _configuration = configuration;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string apiKey = _configuration.GetSection("ApiKey").Value ?? string.Empty;
            if (!string.IsNullOrEmpty(apiKey))
            {
                request.Headers.Add("ApiKey", apiKey);
            }

            if (_authContext != null)
            {
                if (!string.IsNullOrEmpty(_authContext.AccessToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authContext.AccessToken.Replace("Bearer ", ""));
                }

                request.Headers.Add("correlation_id", _authContext.CorrelationId);
            }

            using (_logger.BeginScope("{@CorrelationId} {@AuthContext}", _authContext?.CorrelationId, _authContext))
            {
                _logger.LogInformation($"{request.Method} {request.RequestUri}");
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
        }
    }
}