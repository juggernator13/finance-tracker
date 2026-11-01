using Finance.Web;
using Finance.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<AuthState>();
builder.Services.AddScoped<FinanceApi>();

builder.Services.AddHttpClient("identity", c => c.BaseAddress = new Uri(builder.Configuration["Services:Identity"]!));
builder.Services.AddHttpClient("accounts", c => c.BaseAddress = new Uri(builder.Configuration["Services:Accounts"]!));
builder.Services.AddHttpClient("import", c => c.BaseAddress = new Uri(builder.Configuration["Services:Import"]!));


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
