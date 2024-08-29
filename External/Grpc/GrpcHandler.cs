namespace Muonroi.BuildingBlock.External.Grpc
{
    public static class GrpcHandler
    {
        // Register gRPC server with GrpcServerInterceptor
        public static void AddGrpcServer(this IServiceCollection services)
        {
            _ = services.AddSingleton<GrpcServerInterceptor>();
            _ = services.AddGrpc(options =>
            {
                options.Interceptors.Add<GrpcServerInterceptor>(); // Add the server interceptor
                options.EnableDetailedErrors = true;
                options.MaxSendMessageSize = 100 * 1024 * 1024; // 100 MB
                options.MaxReceiveMessageSize = 100 * 1024 * 1024; // 100 MB
                options.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Optimal;
                options.ResponseCompressionAlgorithm = "gzip";
            });
        }

        // Register a single gRPC client with basic configurations
        public static IHttpClientBuilder AddGrpcClient<TClient>(this IServiceCollection services, string serviceUri)
            where TClient : class
        {
            return services.AddGrpcClient<TClient>(o =>
            {
                o.Address = new Uri(serviceUri);
            })
            .AddInterceptor<GrpcServerInterceptor>();
        }

        // Register multiple gRPC clients using GrpcServicesConfig
        public static IServiceCollection AddGrpcClients(
            this IServiceCollection services,
            IConfiguration configuration,
            Dictionary<string, Type> clients)
        {
            GrpcServicesConfig grpcServicesConfig = new();
            configuration.GetSection(nameof(GrpcServicesConfig)).Bind(grpcServicesConfig);

            foreach (KeyValuePair<string, Type> client in clients)
            {
                if (!grpcServicesConfig.Services.TryGetValue(client.Key, out GrpcServiceConfig? serviceConfig) || string.IsNullOrEmpty(serviceConfig.Uri))
                {
                    throw new KeyNotFoundException($"Service '{client.Key}' not found or Uri is not configured in GrpcServices.");
                }

                _ = services.AddGrpcClient(client.Value, serviceConfig.Uri);
            }

            return services;
        }

        private static IHttpClientBuilder AddGrpcClient(this IServiceCollection services, Type clientType, string serviceUri)
        {
            MethodInfo method = typeof(GrpcHandler)
                .GetMethod(nameof(AddGrpcClient), BindingFlags.Public | BindingFlags.Static, [typeof(IServiceCollection), typeof(string)])!
                .MakeGenericMethod(clientType);

            // Invoke the method and check for null
            object? result = method.Invoke(null, [services, serviceUri]);

            // Handle the null case appropriately
            return result as IHttpClientBuilder
                ?? throw new InvalidOperationException("Failed to create the gRPC client builder.");
        }
    }
}