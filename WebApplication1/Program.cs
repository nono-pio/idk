using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using WebApplication1.apis;

var builder = WebApplication.CreateBuilder(args);

// Swagger Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CAS API", Description = "CAS du turflu", Version = "v1" });
});

builder.Services.AddControllers();

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

// Set up Swagger
//if (app.Environment.IsDevelopment())
if (true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CAS API V1");
    });
}

app.UseHttpsRedirection();

// cas api
var apiRouteBuilder = app.MapGroup("/api");
Api.APIRoutes(apiRouteBuilder);

// math error manager
apiRouteBuilder.MapPost("/error", ([FromBody] MathError request) => { 
    MathErrorController.AddError(request);
    return Results.Ok();
});
apiRouteBuilder.MapGet("/error", () => Results.Ok(MathErrorController.GetErrors()));


app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();


