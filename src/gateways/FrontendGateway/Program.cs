using FrontendGateway;
using Serilog;
using Serilog.Events;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureAppConfiguration((hostingContext, config) =>
{
    config
        .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
        .AddJsonFile("appsettings.json", true, true)
        .AddJsonFile("ocelot.json", false, true)
        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true)
        .AddJsonFile($"ocelot.{hostingContext.HostingEnvironment.EnvironmentName}.json",
         optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    if (hostingContext.HostingEnvironment.EnvironmentName == "Development")
    {
        config.AddJsonFile("appsettings.Local.json", true, true);
    }
});

builder.UseSerilog((_, config) =>
{
    config
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(@"Logs\store.log");
});

builder.ConfigureWebHostDefaults(webBuilder =>
{
    webBuilder.UseStartup<Startup>();
});

builder.Build().Run();
