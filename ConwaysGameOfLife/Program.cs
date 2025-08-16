using ConwaysGameOfLife.Data;
using ConwaysGameOfLife.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NetCore.AutoRegisterDi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using TGolla.Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

var assembliesToScan = new[]
{
    Assembly.GetAssembly(typeof(ConwaysGameOfLifeService))
};

#if DEBUG
var serilogDebugLogFile = File.AppendText("logs/Serilog.SelfLog.txt");
Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(serilogDebugLogFile));
#endif

builder.Services.RegisterAssemblyPublicNonGenericClasses(assembliesToScan)
    .Where(c => c.Name.EndsWith("Service"))
    .AsPublicImplementedInterfaces();

// Add services to the container.
builder.Services.AddDatabaseDeveloperPageExceptionFilter(); // Only necessary with Microsoft.EntityFrameworkCore https://github.com/aspnet/Announcements/issues/432
builder.Services.AddDbContext<ConwaysGameOfLifeApiDbContext>(options =>
     options.UseSqlServer(configuration.GetConnectionString("ConwaysGameOfLifeApiDb")));

builder.Services.AddApplicationInsightsTelemetry();

// Add authentication and authorization here.

SwaggerControllerOrder<ControllerBase> swaggerControllerOrder = new SwaggerControllerOrder<ControllerBase>(Assembly.GetEntryAssembly());
builder.Services.AddSwaggerGen(delegate (SwaggerGenOptions options)
{
    options.OrderActionsBy((ApiDescription apiDesc) => swaggerControllerOrder.SortKey(apiDesc.ActionDescriptor.RouteValues["controller"]) + "_" + apiDesc.RelativePath);
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Conway's Game of Life",
        Version = "v1"
    });
    string path = "ConwaysGameOfLife.xml";
    string filePath = Path.Combine(AppContext.BaseDirectory, path);
    options.IncludeXmlComments(filePath);
    options.EnableAnnotations();
    options.OperationFilter<AppendAuthorizationToDescription>(new object[1] { true });
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();

if (!app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
