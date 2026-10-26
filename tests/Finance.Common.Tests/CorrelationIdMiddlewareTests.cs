using Microsoft.AspNetCore.Http;

namespace Finance.Common.Tests;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task Creates_id_when_header_is_missing()
    {
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        var id = context.Response.Headers["X-Correlation-Id"].ToString();
        Assert.False(string.IsNullOrEmpty(id));
    }

    [Fact]
    public async Task Keeps_id_from_request_header()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-Id"] = "abc-123";
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal("abc-123", context.Response.Headers["X-Correlation-Id"].ToString());
    }
}