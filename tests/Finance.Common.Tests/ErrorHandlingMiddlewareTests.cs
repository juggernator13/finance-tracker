using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Finance.Common.Tests;

public class ErrorHandlingMiddlewareTests
{
    private static async Task<(int Status, string Body)> RunWithException(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.RequestServices = new ServiceCollection().BuildServiceProvider();

        var middleware = new ErrorHandlingMiddleware(
            _ => throw exception,
            NullLogger<ErrorHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return (context.Response.StatusCode, body);
    }

    [Fact]
    public async Task Returns_404_for_KeyNotFoundException()
    {
        var (status, _) = await RunWithException(new KeyNotFoundException("Account not found"));

        Assert.Equal(404, status);
    }

    [Fact]
    public async Task Returns_500_and_hides_internal_message_for_other_exceptions()
    {
        var (status, body) = await RunWithException(new Exception("secret password"));

        Assert.Equal(500, status);
        Assert.DoesNotContain("secret", body);
    }
}