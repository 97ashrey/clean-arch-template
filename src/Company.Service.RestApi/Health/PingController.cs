using Company.Service.RestApi.Common.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Company.Service.RestApi.Health;

public class PingController(ILogger<PingController> logger) : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> HealthCheck(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Checking {ServiceName} health...", Assembly.GetExecutingAssembly().GetName().Name);

        await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

        return Ok();
    }
}
