# VersionedOpenApi
An example of how to use OpenAPI to document .NET web APIs that have multiple endpoint versions.

> :warning: **Disclaimer:** The content of this repository is focused *exclusively* on teaching how to add OpenAPI support to a .NET web API that has multiple endpoint versions. Anything beyond that scope is intentionally not present in order to make clear what is really necessary.

## OpenAPI
To add OpenAPI support to our web API, we first need to add the package `Microsoft.AspNetCore.OpenApi`.

```
dotnet add package Microsoft.AspNetCore.OpenApi
```

Our API is going to have v1 and v2 endpoints, so we use `AddOpenApi` in `Program` to generate an OpenAPI documentation for each named after the version.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOpenApi()
    .AddOpenApi("v2");
```

We then install the package `Asp.Versioning.Http` to use `AddApiVersioning` to add API versioning services. We also install the package `Asp.Versioning.Mvc.ApiExplorer` to add an explorer using `AddApiExplorer` specifying the version format and that we want it to be substituted in the URL.

```csharp
builder.Services.AddApiVersioning()
    .AddApiExplorer(options =>
    {
        // Specifies the format of the version
        options.GroupNameFormat = "'v'VVV";

        // Substitutes "v{version:apiVersion}" for the actual version
        // in the endpoints route shown in the documentation,
        // i.e. instead of "v{version:apiVersion}/version"
        // it will be "v1/version"
        options.SubstituteApiVersionInUrl = true;
    });
```

Now we use `MapOpenApi` to add an endpoint where the OpenAPI documentation will be available. We will use the default route ("/openapi/{documentName}.json").

```csharp
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Adds the OpenAPI documentation endpoint
    app.MapOpenApi();
}
```

The documentation should now be available at "/openapi/v1.json" and "/openapi/v2.json". All we need now are endpoints!

### Minimal API
Let's add an endpoint that returns it's version, either v1 or v2.

```csharp
// Creates a new versioned API
var versionedApi = app.NewVersionedApi();

// Creates a group of endpoints
var group = versionedApi
    .MapGroup("v{version:apiVersion}/version")
    .HasApiVersion(1)
    .HasApiVersion(2);

// Adds endpoints to the group
group.MapGet("", () => new { version = "v1" })
    .WithName("VersionReporterV1")
    .MapToApiVersion(1);

group.MapGet("", () => new { version = "v2" })
    .WithName("VersionReporterV2")
    .MapToApiVersion(2);
```

### Controller
`Program` should have support for controllers.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// ...

app.MapControllers();
```

Now we need a controller with our endpoints. Let's use the same from our Minimal API example.

```csharp
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion(1)]
[ApiVersion(2)]
public class VersionController : ControllerBase
{
    [HttpGet(Name = "VersionReporterV1")]
    [MapToApiVersion(1)]
    public VersionResponse Get() => new("v1");

    [HttpGet(Name = "VersionReporterV2")]
    [MapToApiVersion(2)]
    public VersionResponse GetV2() => new("v2");
}

public sealed record VersionResponse(string Version);
```

So we now have our API documentation available as a JSON, including our endpoints. Let's use it to make it available in not only a more human-readable format, but also make it easier to send requests to our API straight from the browser.

## Scalar
To add [Scalar](https://scalar.com/), install the package `Scalar.AspNetCore`.

```
dotnet add package Scalar.AspNetCore
```

Then add it to the API using `MapScalarApiReference`.

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
```

The Scalar endpoint should be accessible at "/scalar/v1" and "/scalar/v2".

## Swagger
To add [Swagger](https://swagger.io/), install the package `Swashbuckle.AspNetCore`.

```
dotnet add package Swashbuckle.AspNetCore
```

Use `AddSwaggerGen` to inject some services needed for the Swagger middleware.

```csharp
builder.Services.AddSwaggerGen();
```

Then use `UseSwagger` to register the Swagger middleware and `UseSwaggerUI` to tell Swagger to use the OpenAPI documentation in the provided path. `DescribeApiVersions` returns all the available versions, so we iterate over them, adding individual Swagger endpoints for each. This way, we don't need to declare them manually, which would be prone to errors.

```csharp
if (app.Environment.IsDevelopment())
{
    // Adds the OpenAPI documentation endpoint
    app.MapOpenApi();
    // Register the Swagger middleware
    app.UseSwagger();
    // Adds the Swagger page
    app.UseSwaggerUI(options =>
    {
        // Adds a Swagger endpoint for each version
        foreach (var description in app.DescribeApiVersions())
        {
            options.SwaggerEndpoint(
                $"/openapi/{description.GroupName}.json",
                description.GroupName);
        }
    });
}
```

> :warning: In Minimal APIs, the endpoints need to be added **before** calling `DescribeApiVersions`, since it returns all versions added to `app`.

The Swagger endpoint should be accessible at "/swagger".
