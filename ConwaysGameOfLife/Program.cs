using ConwaysGameOfLife.Data;
using ConwaysGameOfLife.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using NetCore.AutoRegisterDi;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using TGolla.Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

if (!builder.Environment.IsProduction())
{
    builder.Configuration.AddUserSecrets<Program>();
}

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

//builder.Services.AddApplicationInsightsTelemetry();

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

builder.Services.AddOpenApi();

var app = builder.Build();

// This example is littered with multiple ways the generate an OpenAPI JSON file.
// With .NeT 10 Microsoft recomends using MapOpenApi() (Microsoft.AspNetCore.OpenApi) instead of Swashbuckle.AspNetCore AddSwaggerGen() and app.MapSwagger().
app.MapOpenApi();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();

if (!app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// If you do want to keep using Swashbuckle.AspNetCore you need the following lines.
app.MapSwagger();
app.UseSwaggerUI();

// If you still want to use   but with Microsoft.AspNetCore.OpenApi added the following line
// Note: Normally it's not necessary to define RoutePrefix as the Swagger UI will default to /swagger.
// In this example with two defined Swagger UIs RoutePrefix is necessary to browser to the Microsoft OpenAPI at /swaggerOpenApi.
app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "swaggerOpenApi";
    c.SwaggerEndpoint("/OpenApi/v1.json", "Microsoft OpenAPI");
    c.DocumentTitle = "Microsoft OpenAPI";
});

app.UseReDoc(options =>
{
    options.SpecUrl = "/OpenApi/v1.json";
});

app.MapScalarApiReference();

app.MapControllers();

app.Run();
