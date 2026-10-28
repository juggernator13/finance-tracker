using Finance.Common;
using Finance.Identity.Data;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.AddFinanceSerilog();
builder.Services.AddControllers();
builder.Services.AddFinanceJwt(builder.Configuration);
builder.Services.AddFinanceSwagger("Finance Identity API");
builder.Services.AddHealthChecks().AddDbContextCheck<IdentityDbContext>();

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    db.Database.Migrate();
}

app.UseCorrelationId();
app.UseErrorHandling();
app.UseFinanceSwagger();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
