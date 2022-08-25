using ShounenGaming.Common;
using System.Reflection;

//Services
var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureServices(builder.Configuration, builder.Environment, Assembly.GetExecutingAssembly().GetName().Name);

//App
var app = builder.Build();
app.ConfigureApp();

