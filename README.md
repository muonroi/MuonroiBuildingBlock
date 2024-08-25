
# **API Library with Dependency Injection and Serilog Configuration**

## **Introduction**
This library provides entities such as `User`, `Role`, `Permission`, and `Language`, and comes with built-in Dependency Injection features, Bearer Token management, JSON handling utilities, string conversion, and localization for multiple languages.

## **Usage Guide**

### **1. Configuring `Program.cs`**

Here's how to configure your `Program.cs` file:

```csharp
using Serilog;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add application configurations
_ = builder.AddAppConfigurations();

// Add Autofac configuration
_ = builder.AddAutofacConfiguration();

// Configure Serilog
_ = builder.Host.UseSerilog(MSerilogAction.Configure);

Log.Information("Starting {ApplicationName} API up", builder.Environment.ApplicationName);

try
{
    IConfiguration configuration = builder.Configuration;
    Assembly assembly = Assembly.GetExecutingAssembly();
    IServiceCollection services = builder.Services;

    // Add services to DI container
    _ = services.AddApplication(assembly);
    _ = services.AddInfrastructure(configuration);
    _ = services.AddDbContextConfigure<ApiDbContext>(configuration);
    _ = services.SwaggerConfig(builder.Environment.ApplicationName);
    _ = services.AddScopeServices(typeof(ApiDbContext).Assembly);
    _ = services.AddValidateBearerToken<MTokenInfo>();

    WebApplication app = builder.Build();

    // Add localization and middleware
    _ = app.AddLocalization(assembly);
    _ = app.UseRouting();
    _ = app.UseAuthentication();
    _ = app.UseAuthorization();
    _ = app.ConfigureEndpoints();
    _ = app.MigrateDatabase();
    _ = app.UseMiddleware<MExceptionMiddleware>();

    await app.RunAsync();
}
catch (Exception ex)
{
    string type = ex.GetType().Name;

    if (type.Equals("StopTheHostException", StringComparison.Ordinal))
    {
        throw;
    }

    Log.Fatal(ex, "Unhandled exception: ", ex.Message);
}
finally
{
    Log.Information("Shut down ", builder.Environment.ApplicationName + " complete");
    await Log.CloseAndFlushAsync();
}
```

### **2. Example `appsettings.json` Configuration**

Here's an example of the `appsettings.json` configuration that can be used with this library:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "ApiKey": "",
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
