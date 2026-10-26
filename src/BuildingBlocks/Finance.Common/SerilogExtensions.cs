using Microsoft.AspNetCore.Builder;

using Serilog;

namespace Finance.Common;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddFinanceSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, config) => config
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}"));

        return builder;
    }
}
