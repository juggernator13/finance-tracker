using Finance.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFinanceJwt(builder.Configuration);
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCorrelationId();

app.UseAuthentication();

app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapReverseProxy();

app.Run();