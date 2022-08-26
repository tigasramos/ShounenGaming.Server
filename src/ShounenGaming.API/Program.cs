using Serilog;
using ShounenGaming.Common;
using System.Reflection;

try
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Override("Default", Serilog.Events.LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File($"logs/log-.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();


    Log.Information("Starting Shounen Gaming Server");

    //Services
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog(); 
    builder.Services.ConfigureServices(builder.Configuration, builder.Environment, Assembly.GetExecutingAssembly().GetName().Name);

    //App
    var app = builder.Build();
    app.ConfigureApp();


}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

