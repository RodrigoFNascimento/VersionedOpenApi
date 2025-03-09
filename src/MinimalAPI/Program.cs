using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// Generates OpenAPI documents for each endpoint version
// The version is the document name
builder.Services
    .AddOpenApi("v1")
    .AddOpenApi("v2");

// Adds support to versioning to the API
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Adds the OpenAPI documentation endpoint
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Creates a group of endpoints
var group = app.NewVersionedApi()
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

app.Run();
