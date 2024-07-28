namespace MBuildingBlock.External.Common
{
    public static class MHelpers
    {
        private static ConcurrentDictionary<string, Dictionary<string, string>> ErrorMessages = new();
        private static IMJsonSerializeService? _serializeService;
        private static ResourceSetting? _resourceSetting;
        private static Assembly? _resourceAssembly;

        static MHelpers()
        {
        }

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
                throw new Exception("SecretKey cannot be an empty string");
            }

            string configValue = keyOfConfig;
            string descryptConnectionString = configuration.GetCryptConfigValue(configValue, secretKey)!;
            return string.IsNullOrEmpty(descryptConnectionString)
                ? throw new Exception("configValue cannot be an empty string")
                : descryptConnectionString;
        }

        public static string GenerateErrorResult(string propertyName, object propertyValue)
        {
            return $"{char.ToLowerInvariant(propertyName[0])}{propertyName[1..]}: {propertyValue}";
        }

        public static string GetErrorMessage(string errorCode, string lang = "en-US")
        {
            lang = lang.GetSettingValue();
            return GetErrorMessage(errorCode, ref ErrorMessages, lang);
        }

        public static string GetErrorMessage(string errorCode, ref ConcurrentDictionary<string, Dictionary<string, string>> errorMessages, string lang = "en-US")
        {
            lang = lang.GetSettingValue();
            const string defaultMessage = "No pre-defined error message";

            if (_resourceAssembly == null)
            {
                return defaultMessage;
            }

            string fileName = $"{_resourceAssembly.GetName().Name}@@{lang}";

            if (errorMessages.TryGetValue(fileName, out Dictionary<string, string>? dictionary))
            {
                return dictionary != null && dictionary.TryGetValue(errorCode, out string? value) ? value : defaultMessage;
            }

            dictionary ??= LoadErrorMessages(_resourceAssembly, lang);
            SetErrorMessagesOfLanguage(ref errorMessages, fileName, dictionary);

            return dictionary.TryGetValue(errorCode, out string? errorMsg) ? errorMsg : defaultMessage;
        }

        private static Dictionary<string, string> LoadErrorMessages(Assembly resourceAssembly, string lang)
        {
            try
            {
                string resourceName = $"{nameof(SystemSettingKey.ResourceName).GetSettingValue()}-{lang}.json";
                string fromResources = GetFromResources(resourceName, resourceAssembly);
                return _serializeService!.Deserialize<Dictionary<string, string>>(fromResources) ?? [];
            }
            catch
            {
                return [];
            }
        }

        private static void SetErrorMessagesOfLanguage(ref ConcurrentDictionary<string, Dictionary<string, string>> errorsList, string language, Dictionary<string, string> errors)
        {
            errorsList[language] = errors;
        }

        public static string GetFromResources(string resourceName, Assembly resourceAssembly)
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