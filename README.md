
# **API Library with Dependency Injection and Serilog Configuration**

## **Introduction**
This library provides entities such as `User`, `Role`, `Permission`, and `Language`, and comes with built-in Dependency Injection features, Bearer Token management, JSON handling utilities, string conversion, and localization for multiple languages.

## **Usage Guide**

### **1. Configuring `Program.cs`**

Here's how to configure your `Program.cs` file:

```csharp




WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
Assembly assembly = Assembly.GetExecutingAssembly();
ConfigurationManager configuration = builder.Configuration;

builder.AddAppConfigurations();
builder.AddAutofacConfiguration();
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    MSerilogAction.Configure(context, services, loggerConfiguration, false);
});

Log.Information("Starting {ApplicationName} API up", builder.Environment.ApplicationName);

try
{
    IServiceCollection services = builder.Services;

    // Register services
    _ = services.AddApplication(assembly);
    _ = services.AddInfrastructure<Program>(configuration);
    _ = services.SwaggerConfig(builder.Environment.ApplicationName);
    _ = services.AddScopeServices(typeof(<Your Db context>).Assembly);
    _ = services.AddValidateBearerToken<MTokenInfo, <Your Enum permission>>(configuration);
    _ = services.AddDbContextConfigure<<Your Db context>, <Your Enum permission>>(configuration);
    _ = services.AddCors(configuration);
    _ = services.AddPermissionFilter<<Your Enum permission>>();
    _ = services.ConfigureMapper();
    WebApplication app = builder.Build();
    _ = app.UseCors("MAllowDomains");
    _ = app.UseDefaultMiddleware<<Your Db context>, <Your Enum permission>>();
    _ = app.AddLocalization(assembly);
    _ = app.UseRouting();
    _ = app.UseAuthentication();
    _ = app.UseAuthorization();
    _ = app.ConfigureEndpoints();
    _ = app.MigrateDatabase<<Your Db context>>();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception: {Message}", ex.Message);
}
finally
{
    Log.Information("Shut down {ApplicationName} complete", builder.Environment.ApplicationName);
    await Log.CloseAndFlushAsync();
}
```

### **2. Example `appsettings.json` Configuration**

Here's an example of the `appsettings.json` configuration that can be used with this library:

```json
{
  "DatabaseConfigs": {
    "DbType": "Sqlite",
    "ConnectionStrings": {
      "SqliteConnectionString": "Your encrypt connection string by serect key",
      "MongoDbConnectionString": "Your encrypt connection string by serect key",
      "SqlServerConnectionString": "Your encrypt connection string by serect key",
      "MySqlConnectionString": "Your encrypt connection string by serect key",
      "PostgreSqlConnectionString": "Your encrypt connection string by serect key"
    }
  },
  "ApiKey": "",
    "RedisConfigs": {
    "Enable": true,
    "Host": "Your encrypt by serect key",
    "Port": "Your encrypt by serect key",
    "Password": "Your encrypt by serect key",
    "Expire": 30,
    "KeyPrefix": "Your encrypt by serect key",
    "AllMethodsEnableCache": false
  },
  "TokenConfigs": {
    "Issuer": "https://exampledomain.com",
    "Audience": "https://searchpartners.exampledomain.com",
    "SigningKeys": "",
    "ExpiryMinutes": 30,
    "PublicKey": "-----BEGIN PUBLIC KEY-----\n-----END PUBLIC KEY-----",
    "PrivateKey": "-----BEGIN RSA PRIVATE KEY-----

-----END RSA PRIVATE KEY-----"
  },
  "PaginationConfigs": {
    "DefaultPageIndex": 1,
    "DefaultPageSize": 10,
    "MaxPageSize": 10
  },
  "ResourceSetting": {
    "ResourceName": "Resources.ErrorMessages",
    "lang": "vi-VN"
  },
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.Elasticsearch" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    },
    "MAllowDomains": "https://localhost:52182,http://localhost:4200",
    "GrpcServices": {
    "Services": {
      "Your service 1": {
        "Uri": "Your service url"
      },
      "Your service 2": {
        "Uri": "Your service url"
      },
      "Your service 3": {
        "Uri": "Your service url"
      }
    }
  },
    "ConsulConfigs": {
    "ServiceName": "MyService",
    "ConsulAddress": "Your service url",
    "ServiceAddress": "http://localhost",
    "ServicePort": 5000,
    "ServiceMetadata": {
      "version": "1.0.0",
      "environment": "production"
    }
  },
  ApiKey: "Your api key use with grpc",
    "SecretKey": "Your serect key use encrypt impotant value",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "Elasticsearch",
        "Args": {
          "nodeUris": "http://localhost:9200",
          "autoRegisterTemplate": true,
          "indexFormat": "notifications_system-{0:yyyy.MM.dd}",
          "inlineFields": true,
          "numberOfShards": 2,
          "numberOfReplicas": 2,
          "autoRegisterTemplateVersion": "ESv7"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ],
    "Properties": {
      "Application": "Notifications"
    }
  },
  "ErrorInAverageEveryNTime": 2,
  "WarningInAverageEveryNTime": 2
}
```

## **Main Components**

### **1. Entities**
- **User**: Manages users.
- **Role**: Manages user roles.
- **Permission**: Manages access permissions.
- **Language**: Manages languages.

### **2. Dependency Injection**
- Integrates necessary services via Dependency Injection (DI) to manage the application lifecycle.

### **3. Bearer Token**
- Manages Bearer Token, authentication, and authorization.

### **4. JSON Utilities**
- Provides utilities for handling JSON, such as converting between strings and objects.

### **5. Localization**
- Supports language localization within the application for various languages.

## **Contribution**
Please submit pull requests or open issues on GitHub to contribute or report bugs for this project.

## **License**
This library is licensed under the MIT License. Please see the `LICENSE` file for more details.

---

Please customize the configuration details (such as `ConnectionStrings`, `ApiKey`, `TokenConfigs`, etc.) according to your application�s requirements.
