using FastEndpointDemo.Services;
using FastEndpointDemo.Services.Interfaces;
using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddFastEndpoints()
    .SwaggerDocument(); 

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<IPersonStorageService, PersonMemoryCacheStorageService>();
builder.Services.AddHostedService<PersonStorageInitializerService>();

var app = builder.Build();
app.UseHttpsRedirection();

app.UseDefaultExceptionHandler()
    .UseFastEndpoints()
    .UseSwaggerGen();
app.Run();

// Make Program accessible to tests
namespace FastEndpointDemo
{
    public partial class Program { }
}
