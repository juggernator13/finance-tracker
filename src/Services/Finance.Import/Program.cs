using Finance.Common;
using Finance.Contracts;
using Finance.Import;
using Finance.Messaging;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ImportDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.AddFinanceSerilog();
builder.Services.AddControllers();
builder.Services.AddFinanceJwt(builder.Configuration);
builder.Services.AddFinanceSwagger("Finance Import API");
builder.Services.AddHealthChecks().AddDbContextCheck<ImportDbContext>();

builder.Services.AddSingleton<IStatementParser, Bank1CsvParser>();
builder.Services.AddSingleton<IStatementParser, Bank2CsvParser>();
builder.Services.AddSingleton<StatementParserFactory>();

builder.Services.AddHttpClient<IAccountsClient, AccountsClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Services:Accounts:BaseUrl"]!));

builder.Services.AddScoped<ImportService>();

builder.Services.AddRabbitMq(builder.Configuration);
builder.Services.AddMessageHandler<TransactionsCategorized, TransactionsCategorizedHandler>("import");
builder.Services.AddMessageHandler<CategorizationFailed, CategorizationFailedHandler>("import");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ImportDbContext>();
    db.Database.Migrate();
}

app.UseFinanceSwagger();

app.MapControllers();

app.MapHealthChecks("/health");

app.UseAuthorization();

app.MapControllers();

app.Run();