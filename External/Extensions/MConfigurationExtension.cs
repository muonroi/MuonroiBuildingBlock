﻿namespace Muonroi.BuildingBlock.External.Extensions;

public static class MConfigurationExtension
{
    private const string ConfigKey = "SecretKey";

    public static T GetOptions<T>(this IServiceCollection services, string sectionName)
        where T : new()
    {
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
        IConfigurationSection section = configuration.GetSection(sectionName);
        T options = new();
        section.Bind(options);

        return options;
    }

    public static T GetOptions<T>(this IConfiguration configuration, string section)
        where T : new()
    {
        T model = new();
        configuration.GetSection(section).Bind(model);
        return model;
    }

    public static string? GetCryptConfigValue(this IConfiguration configuration, string configKey
        , string secretKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(configKey);
        ArgumentException.ThrowIfNullOrEmpty(secretKey);

        string? cipherText = configuration.GetCryptConfigValue(configKey);
        ArgumentException.ThrowIfNullOrEmpty(cipherText);

        string planText = MCryptographyExtension.Decrypt(secretKey, cipherText);
        ArgumentException.ThrowIfNullOrEmpty(planText);

        return planText;
    }

    public static string? GetCryptConfigValue(this IConfiguration configuration, string configKey
        , bool useConfigureSecretKey = true
        , string secretKey = "")
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(configKey);

        if (useConfigureSecretKey && !string.IsNullOrEmpty(secretKey))
        {
            ArgumentException.ThrowIfNullOrEmpty(secretKey);
        }
        else
        {
            if (!useConfigureSecretKey && string.IsNullOrEmpty(secretKey))
            {
                ArgumentException.ThrowIfNullOrEmpty(secretKey);
            }

            secretKey = configuration.GetCryptConfigValue(ConfigKey) ?? string.Empty;
            ArgumentException.ThrowIfNullOrEmpty(secretKey);
        }

        string? cipherText = configuration.GetCryptConfigValue(configKey);
        ArgumentException.ThrowIfNullOrEmpty(cipherText);

        string planText = MCryptographyExtension.Decrypt(secretKey, cipherText);

        return planText;
    }

    public static string? GetCryptConfigValueCipherText(this IConfiguration configuration, string cipherText)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        string secretKey = configuration.GetCryptConfigValue(ConfigKey) ?? string.Empty;
        ArgumentException.ThrowIfNullOrEmpty(secretKey);

        string planText = MCryptographyExtension.Decrypt(secretKey, cipherText);

        return planText;
    }

    public static string? GetCryptConfigValue(this IConfiguration configuration, string configKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(configKey);

        return configuration[configKey];
    }

    public static IServiceCollection ConfigureDictionary<TOptions>(this IServiceCollection services,
              IConfigurationSection section) where TOptions : class, IDictionary<string, string>
    {
        List<IConfigurationSection> values = section
            .GetChildren()
            .ToList();

        _ = services.Configure<TOptions>(x =>
        values.ForEach(v =>
        {
            if (v is not null)
            {
                x.Add(v.Key, v.Value ?? string.Empty);
            }
        }));

        return services;
    }

    public static TConfig ConfigureStartupConfig<TConfig>(this IServiceCollection services
        , IConfiguration configuration) where TConfig : class, new()
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        TConfig config = new();
        configuration.Bind(config);
        return config;
    }
}