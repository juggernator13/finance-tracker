using Finance.Common;
using Finance.Import;

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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ImportDbContext>();
    db.Database.Migrate();
}

app.UseFinanceSwagger();

app.MapControllers();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();