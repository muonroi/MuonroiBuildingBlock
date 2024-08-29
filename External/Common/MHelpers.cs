namespace Muonroi.BuildingBlock.External.Common
{
    public static class MHelpers
    {
        private static readonly ConcurrentDictionary<string, Dictionary<string, string>> ErrorMessages = new();
        private static IMJsonSerializeService? _serializeService;
        private static ResourceSetting? _resourceSetting;
        private static Assembly? _resourceAssembly;

        public static void Initialize(ResourceSetting resourceSetting, IMJsonSerializeService serializeService, Assembly resourceAssembly)
        {
            _resourceSetting = resourceSetting;
            _serializeService = serializeService;
            _resourceAssembly = resourceAssembly;
        }

        public static string GetConfigHelper(this IConfiguration configuration, string keyOfConfig)
        {
            string? secretKey = configuration.GetCryptConfigValue("SecretKey");
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("SecretKey cannot be an empty string");
            }

            string? configValue = configuration.GetCryptConfigValue(keyOfConfig, secretKey);
            return string.IsNullOrEmpty(configValue)
                ? throw new InvalidOperationException("Config value cannot be an empty string")
                : configValue;
        }

        public static string GenerateErrorResult(string propertyName, object propertyValue)
        {
            return $"{char.ToLowerInvariant(propertyName[0])}{propertyName[1..]}: {propertyValue}";
        }

        public static string GetErrorMessage(string errorCode, string lang = "en-US")
        {
            lang = lang.GetSettingValue();
            return GetErrorMessageInternal(errorCode, lang);
        }

        private static string GetErrorMessageInternal(string errorCode, string lang)
        {
            lang = lang.GetSettingValue();
            const string defaultMessage = "No pre-defined error message";

            if (_resourceAssembly == null)
            {
                return defaultMessage;
            }

            string fileName = $"{_resourceAssembly.GetName().Name}@@{lang}";

            if (ErrorMessages.TryGetValue(fileName, out Dictionary<string, string>? dictionary))
            {
                return dictionary != null && dictionary.TryGetValue(errorCode, out string? value) ? value : defaultMessage;
            }

            dictionary = LoadErrorMessages(_resourceAssembly, lang);
            SetErrorMessagesOfLanguage(fileName, dictionary);

            return dictionary.TryGetValue(errorCode, out string? errorMsg) ? errorMsg : defaultMessage;
        }

        private static Dictionary<string, string> LoadErrorMessages(Assembly resourceAssembly, string lang)
        {
            try
            {
                string resourceName = $"{nameof(SystemSettingKey.ResourceName).GetSettingValue()}-{lang}.json";
                string resourceContent = GetFromResources(resourceName, resourceAssembly);
                return _serializeService?.Deserialize<Dictionary<string, string>>(resourceContent) ?? [];
            }
            catch
            {
                return [];
            }
        }

        private static void SetErrorMessagesOfLanguage(string language, Dictionary<string, string> errors)
        {
            ErrorMessages[language] = errors;
        }

        private static string GetFromResources(string resourceName, Assembly resourceAssembly)
        {
            using Stream? stream = resourceAssembly.GetManifestResourceStream($"{resourceAssembly.GetName().Name}.{resourceName}");
            if (stream == null)
            {
                return string.Empty;
            }

            using StreamReader streamReader = new(stream);
            return streamReader.ReadToEnd();
        }

        public static string GetSettingValue(this string key)
        {
            return _resourceSetting != null && _resourceSetting.TryGetValue(key, out string? value) ? value : "vi-VN";
        }
    }
}