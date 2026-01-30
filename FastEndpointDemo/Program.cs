using FastEndpointDemo.Services;
using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddFastEndpoints()
    .SwaggerDocument(); 

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IPersonStorageService, PersonStorageService>();
builder.Services.AddHostedService<PersonStorageInitializerService>();

var app = builder.Build();
app.UseHttpsRedirection();

app.UseDefaultExceptionHandler()
    .UseFastEndpoints()
    .UseSwaggerGen();
app.Run();

