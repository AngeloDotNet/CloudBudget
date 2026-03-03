using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace CloudBudget.API.Options;

// Small helper used only when JWT is not configured: a NoOp validator to avoid framework throwing missing options errors.
// In production you should remove this and require a valid JWT configuration.
internal sealed class NoOpValidateOptions : IValidateOptions<JwtBearerOptions>
{
    public ValidateOptionsResult Validate(string name, JwtBearerOptions options) => ValidateOptionsResult.Success;
}