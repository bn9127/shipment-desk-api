using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ShipmentDesk.Api.Security;

public sealed class ApiKeyFilter : IAsyncActionFilter
{
    public const string HeaderName = "X-Api-Key";

    private readonly IConfiguration _configuration;

    public ApiKeyFilter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var expected = _configuration["Demo:ApiKey"];
        if (string.IsNullOrEmpty(expected)
            || !context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var provided)
            || !FixedEquals(provided.ToString(), expected))
        {
            context.Result = new UnauthorizedObjectResult(new { error = "A valid X-Api-Key header is required." });
            return;
        }

        await next();
    }

    private static bool FixedEquals(string provided, string expected)
    {
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        return providedBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }
}
