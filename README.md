# VersionedOpenApi
An example of how to use OpenAPI to document .NET web APIs that have multiple endpoint versions.

## OpenAPI
To add OpenAPI support to our web API, we first need to add the package `Microsoft.AspNetCore.OpenApi`.

```csharp
dotnet add package Microsoft.AspNetCore.OpenApi
```

Our API is going to have v1 and v2 endpoints, so we use `AddOpenApi` in `Program` to generate an OpenAPI documentation for each named after the version.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOpenApi("v1")
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

All we need now are endpoints!

### Minimal API
Let's add an endpoint that returns it's version, either v1 or v2.

```csharp
// Creates a new versioned API
var versionedApi = app.NewVersionedApi();

// Creates a group of endpoints
var group = versionedApi
    .MapGroup("v{version:apiVersion}/version")
    .HasApiVersion(1)
    .HasApiVersion(2)
    .WithOpenApi();

// Adds endpoints to the group
group.MapGet("", () => new { version = "v1" })
    .WithName("VersionReporterV1");

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

app.MapControllers()
    .WithOpenApi();
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
    public VersionResponse Get() => new("v1");

    [HttpGet(Name = "VersionReporterV2")]
    [MapToApiVersion(2)]
    public VersionResponse GetV2() => new("v2");
}

public sealed record VersionResponse(string Version);
```

