using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace OrderLunch.Functions;

public class HealthCheckFunction
{
    [Function(nameof(HealchCheck))]
    public IActionResult HealchCheck(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData request)
    {
        return new OkObjectResult("OK");
    }
}
