using Serilog;
using ShounenGaming.Common;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using System.Net;
using Serilog.Exceptions;

try
{
    // To Work without HTTPS
    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
    
    //Services
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
            optional: true,
            reloadOnChange: true)
        .AddEnvironmentVariables();

    builder.Host.UseSerilog((context, loggerConfig) =>
    {
        loggerConfig
            .MinimumLevel.Override("Default", Serilog.Events.LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("ApplicationName", "ShounenGaming.API")
            .WriteTo.Console()
            .WriteTo.File($"logs/log-.txt", rollingInterval: RollingInterval.Day)
            .WriteTo.Seq(context.Configuration["Settings:SeqServer"] ?? "localhost:5341");
    });


    Log.Information("Starting Shounen Gaming Server");

    builder.Services.ConfigureServices(builder.Configuration, builder.Environment, Assembly.GetExecutingAssembly().GetName().Name!);

    //App
    var app = builder.Build();

    app.UseCors(corsPolicyBuilder =>
              corsPolicyBuilder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader());

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = ShounenGaming.Business.Helpers.LogsEnricherHelper.Enricher.HttpRequestEnricher;
    });

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

