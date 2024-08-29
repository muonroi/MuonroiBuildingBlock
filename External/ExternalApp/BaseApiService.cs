namespace Muonroi.BuildingBlock.External.ExternalApp
{
    public abstract class BaseApiService(MAuthenticateInfoContext authContext)
    {
        protected readonly MAuthenticateInfoContext _authContext = authContext;

        protected T CreateClient<T>(string baseUrl) where T : class
        {
            CorrelationIdHandler handler = new(_authContext)
            {
                InnerHandler = new HttpClientHandler()
            };

            HttpClient httpClient = new(handler)
            {
                BaseAddress = new Uri(baseUrl)
            };

            return RestClient.For<T>(httpClient);
        }
    }
}