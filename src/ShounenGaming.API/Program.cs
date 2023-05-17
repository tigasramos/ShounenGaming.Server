using Serilog;
using ShounenGaming.Common;
using System.Reflection;
using Microsoft.Extensions.FileProviders;


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
    builder.Services.ConfigureServices(builder.Configuration, builder.Environment, Assembly.GetExecutingAssembly().GetName().Name!);

    //App
    var app = builder.Build();

    //https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-7.0
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
           Path.Combine(builder.Environment.ContentRootPath, "mangas")),
        RequestPath = "/mangas",
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append(
                 "Cache-Control", $"public, max-age={60 * 60 * 24 * 7}");
        }
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

