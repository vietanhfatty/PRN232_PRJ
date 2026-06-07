using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MyProject.WebMvc.Handlers;

public class CookieForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieForwardingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null && httpContext.Request.Headers.TryGetValue("Cookie", out var cookieValues))
        {
            // Remove any existing Cookie headers on the request to avoid duplication
            request.Headers.Remove("Cookie");
            request.Headers.Add("Cookie", cookieValues.ToString());
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
