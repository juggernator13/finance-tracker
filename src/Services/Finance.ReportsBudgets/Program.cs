using Finance.Common;
using Finance.Contracts;
using Finance.Messaging;
using Finance.ReportsBudgets;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddFinanceSerilog();
builder.Services.AddControllers();
builder.Services.AddFinanceJwt(builder.Configuration);
builder.Services.AddFinanceSwagger("Finance Reports API");

builder.Services.AddDbContext<ReportsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddHealthChecks().AddDbContextCheck<ReportsDbContext>();

builder.Services.AddRabbitMq(builder.Configuration);
builder.Services.AddMessageHandler<TransactionsCategorized, TransactionsCategorizedHandler>("reports");

builder.Services.AddScoped<AggregateService>();
builder.Services.AddScoped<BudgetService>();
builder.Services.AddScoped<ReportService>();

builder.Services.AddStackExchangeRedisCache(o =>
{
    o.Configuration = builder.Configuration["Redis:ConnectionString"];
    o.InstanceName = "finance:";
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReportsDbContext>();
    db.Database.Migrate();
}

app.UseCorrelationId();

app.UseErrorHandling();

app.UseFinanceSwagger();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();