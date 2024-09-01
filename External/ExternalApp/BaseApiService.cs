namespace Muonroi.BuildingBlock.External.ExternalApp
{
    public abstract class BaseApiService(MAuthenticateInfoContext authContext)
    {
        protected readonly MAuthenticateInfoContext _authContext = authContext;

        protected T CreateClient<T>(string baseUrl) where T : class
        {
            global::Polly.Retry.AsyncRetryPolicy<HttpResponseMessage> retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError() // Handles HTTP errors (5xx and 408) and network failures
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); // Retry with exponential backoff

            global::Polly.Timeout.AsyncTimeoutPolicy<HttpResponseMessage> timeoutPolicy = global::Polly.Policy.TimeoutAsync<HttpResponseMessage>(10); // Timeout after 10 seconds

            global::Polly.Wrap.AsyncPolicyWrap<HttpResponseMessage> policyWrap = global::Polly.Policy.WrapAsync(retryPolicy, timeoutPolicy); // Combine policies

            CorrelationIdHandler handler = new(_authContext)
            {
                InnerHandler = new PolicyHandler(policyWrap) // Apply the policy
            };

            HttpClient httpClient = new(handler)
            {
                BaseAddress = new Uri(baseUrl)
            };

            return RestClient.For<T>(httpClient);
        }
    }
}