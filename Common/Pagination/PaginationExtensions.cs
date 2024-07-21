namespace MuonroiBuildingBlock.Common.Pagination
{
    public static class PaginationExtensions
    {
        public static IServiceCollection AddPaginationConfigs(this IServiceCollection services, IConfiguration configuration)
        {
            PaginationConfigs paginationConfigs = new();
            configuration.GetSection(PaginationConfigs.SectionName).Bind(paginationConfigs);
            if (paginationConfigs.DefaultPageIndex < 1)
            {
                paginationConfigs.DefaultPageIndex = 1;
            }

            if (paginationConfigs.DefaultPageSize < 1)
            {
                paginationConfigs.DefaultPageSize = 15;
            }

            if (paginationConfigs.MaxPageSize < paginationConfigs.DefaultPageSize)
            {
                paginationConfigs.MaxPageSize = paginationConfigs.DefaultPageSize;
            }

            _ = services.AddSingleton(paginationConfigs);
            return services;
        }
    }
}