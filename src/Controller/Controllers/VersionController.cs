using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers;

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
