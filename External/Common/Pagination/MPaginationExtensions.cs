namespace MBuildingBlock.External.Common.Pagination
{
    public static class MPaginationExtensions
    {
        public static IServiceCollection AddPaginationConfigs<TPaging>(this IServiceCollection services, IConfiguration configuration, TPaging paginationConfigs)
            where TPaging : MPaginationConfig
        {
            configuration.GetSection(paginationConfigs.SectionName).Bind(paginationConfigs);
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