using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddControllers();
// Generates OpenAPI documents for each endpoint version
// The version is the document name
builder.Services
    .AddOpenApi()
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

// Registers some services needed by the Swagger middleware
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Adds the OpenAPI documentation endpoint
    app.MapOpenApi();
    // Adds the Scalar endpoint
    app.MapScalarApiReference();
    // Registers the Swagger middleware
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

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
