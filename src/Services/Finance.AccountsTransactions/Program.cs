using Finance.AccountsTransactions;
using Finance.Common;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.AddFinanceSerilog();
builder.Services.AddControllers();
builder.Services.AddFinanceJwt(builder.Configuration);
builder.Services.AddFinanceSwagger("Finance Accounts API");

builder.Services.AddDbContext<AccountsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddHealthChecks().AddDbContextCheck<AccountsDbContext>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<TransactionService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AccountsDbContext>();
    db.Database.Migrate();
}

app.UseFinanceSwagger();

app.MapControllers();

app.MapHealthChecks("/health");

app.UseAuthorization();

app.MapControllers();

app.Run();
