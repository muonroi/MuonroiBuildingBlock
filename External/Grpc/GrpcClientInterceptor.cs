namespace Muonroi.BuildingBlock.External.Grpc
{
    public class GrpcServerInterceptor(MAuthenticateInfoContext authContextAccessor) : Interceptor
    {
        private readonly MAuthenticateInfoContext _authContext = authContextAccessor ?? throw new ArgumentNullException(nameof(authContextAccessor));

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            // Check for CorrelationId in request metadata
            string? correlationId = context.RequestHeaders.GetValue(CustomHeader.CorrelationId);

            if (string.IsNullOrEmpty(correlationId))
            {
                // If CorrelationId is missing, generate a new one
                correlationId = Guid.NewGuid().ToString();
                _authContext.CorrelationId = correlationId;
            }
            else
            {
                _authContext.CorrelationId = correlationId;
            }

            // Add CorrelationId to the response trailers (optional)
            context.ResponseTrailers.Add(CustomHeader.CorrelationId, correlationId);

            // Continue with the original gRPC call
            return await continuation(request, context);
        }

        // You can override other handlers if needed, for example:
        public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream,
            ServerCallContext context,
            ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            // Similar logic can be added here for Client Streaming
            return await base.ClientStreamingServerHandler(requestStream, context, continuation);
        }

        public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
            TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context,
            ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            // Similar logic can be added here for Server Streaming
            await base.ServerStreamingServerHandler(request, responseStream, context, continuation);
        }

        public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context,
            DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            // Similar logic can be added here for Duplex Streaming
            await base.DuplexStreamingServerHandler(requestStream, responseStream, context, continuation);
        }
    }

    public static class MetadataExtensions
    {
        public static string? GetValue(this Metadata metadata, string key)
        {
            Metadata.Entry? entry = metadata.FirstOrDefault(m => m.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            return entry?.Value;
        }
    }
}