using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Infrastructure.Logging
{
    public static class SerilogConfiguration
    {
        public static void ConfigureLogging(WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            builder.Host.UseSerilog();
        }
    }
}
