namespace Muonroi.BuildingBlock.External.ExternalApp
{
    public abstract class BaseGrpcService(MAuthenticateInfoContext authContext)
    {
        private readonly MAuthenticateInfoContext _authContext = authContext;

        protected Metadata CreateMetadata()
        {
            Metadata metadata = [];

            if (!string.IsNullOrEmpty(_authContext.CorrelationId))
            {
                metadata.Add(CustomHeader.CorrelationId, _authContext.CorrelationId);
            }

            if (!string.IsNullOrEmpty(_authContext.ApiKey))
            {
                metadata.Add(CustomHeader.ApiKey, _authContext.ApiKey);
            }

            return metadata;
        }

        protected async Task<TResponse> CallGrpcServiceAsync<TResponse>(Func<Metadata, Task<TResponse>> grpcCall)
        {
            Metadata metadata = CreateMetadata();
            return await grpcCall(metadata);
        }
    }
}