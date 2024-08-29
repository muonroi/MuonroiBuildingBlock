namespace Muonroi.BuildingBlock.External.Consul
{
    public static class ConsulHandler
    {
        public static IServiceCollection AddServiceDiscovery(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            // Configure Consul settings from configuration
            ConsulConfigs? consulSettings = configuration.GetSection(nameof(ConsulConfigs)).Get<ConsulConfigs>();

            if (consulSettings != null)
            {
                if (string.IsNullOrWhiteSpace(consulSettings.ConsulAddress))
                {
                    throw new InvalidDataException("Consul address is not configured.");
                }

                _ = services.AddSingleton(consulSettings);

                if (!environment.IsDevelopment())
                {
                    _ = services.AddSingleton<IConsulClient, ConsulClient>(_ => new ConsulClient(consulConfig =>
                    {
                        consulConfig.Address = new Uri(consulSettings.ConsulAddress);
                    }));
                }

                return services;
            }

            throw new InvalidDataException("Consul configuration is missing or invalid.");
        }

        public static IApplicationBuilder UseServiceDiscovery(this IApplicationBuilder app, IWebHostEnvironment environment)
        {
            // Skip service registration in development environment
            if (environment.IsDevelopment())
            {
                return app;
            }

            // Get required services
            IConsulClient consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            IHostApplicationLifetime lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
            ConsulConfigs consulSettings = app.ApplicationServices.GetRequiredService<ConsulConfigs>();

            // Determine service address and port
            string? address = consulSettings.ServiceAddress;
            int port = consulSettings.ServicePort;

            if (string.IsNullOrWhiteSpace(address))
            {
                IServerAddressesFeature? features = app.ServerFeatures.Get<IServerAddressesFeature>();
                string? firstAddress = features?.Addresses.FirstOrDefault();

                if (firstAddress != null)
                {
                    Uri uri = new(firstAddress);
                    address = uri.Host;
                    port = uri.Port;
                }
            }

            if (string.IsNullOrWhiteSpace(address) || port == 0)
            {
                throw new InvalidOperationException("Service address or port could not be determined.");
            }

            // Register service with Consul
            AgentServiceRegistration registration = new()
            {
                ID = $"{consulSettings.ServiceName}-{Guid.NewGuid()}",
                Name = consulSettings.ServiceName,
                Address = address,
                Port = port,
                Meta = consulSettings.ServiceMetadata
            };

            consulClient.Agent.ServiceDeregister(registration.ID).Wait();
            consulClient.Agent.ServiceRegister(registration).Wait();

            // Deregister service when application stops
            _ = lifetime.ApplicationStopping.Register(() =>
            {
                consulClient.Agent.ServiceDeregister(registration.ID).Wait();
            });

            return app;
        }
    }
}