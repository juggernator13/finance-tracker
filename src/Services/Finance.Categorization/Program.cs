using Finance.Categorization;
using Finance.Common;
using Finance.Contracts;
using Finance.Messaging;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddFinanceSerilog();
builder.Services.AddControllers();
builder.Services.AddFinanceJwt(builder.Configuration);
builder.Services.AddFinanceSwagger("Finance Categorization API");

builder.Services.AddDbContext<CategorizationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddHealthChecks().AddDbContextCheck<CategorizationDbContext>();

builder.Services.AddRabbitMq(builder.Configuration);
builder.Services.AddMessageHandler<CategorizeBatch, CategorizeBatchHandler>("categorization");

builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<RuleService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CategorizationDbContext>();
    db.Database.Migrate();
    await DbSeeder.SeedAsync(db);
}

app.UseCorrelationId();

app.UseErrorHandling();

app.UseFinanceSwagger();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();