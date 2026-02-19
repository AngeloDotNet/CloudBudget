using CloudBudget.API.Data;
using Microsoft.EntityFrameworkCore;

namespace CloudBudget.API.Middleware;

/// <summary>
/// Middleware che nega richieste con JWT il cui jti è presente nel RevokedJwts store.
/// Deve essere posizionato dopo UseAuthentication() e prima di UseAuthorization().
/// </summary>
public class JwtRevocationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext, CloudBudgetDbContext db)
    {
        // se non authenticated skip (authorization pipeline gestirà)
        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            var jti = httpContext.User.Claims.FirstOrDefault(c => c.Type is "jti" or System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;

            if (!string.IsNullOrEmpty(jti))
            {
                var revoked = await db.RevokedJwts.AsNoTracking().AnyAsync(r => r.Jti == jti);

                if (revoked)
                {
                    // Invalidate request: 401 Unauthorized
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await httpContext.Response.WriteAsync("Token revoked.");
                    return;
                }
            }
        }

        await next(httpContext);
    }
}