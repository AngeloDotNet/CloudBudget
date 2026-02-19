using Microsoft.AspNetCore.Mvc;

namespace CloudBudget.API.Controllers;

[ApiController]
[Route("api/debug")]
public class DebugController : ControllerBase
{
    [HttpGet("ip")]
    public IActionResult GetIp()
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var xff = HttpContext.Request.Headers["X-Forwarded-For"].ToString();
        var proto = HttpContext.Request.Headers["X-Forwarded-Proto"].ToString();
        var xRealIp = HttpContext.Request.Headers["X-Real-IP"].ToString();

        return Ok(new
        {
            RemoteIp = remoteIp,
            XForwardedFor = xff,
            XForwardedProto = proto,
            XRealIp = xRealIp,
            AllRequestHeaders = HttpContext.Request.Headers.ToDictionary(k => k.Key, v => v.Value.ToString())
        });
    }
}