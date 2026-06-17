using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddControllers();

builder.WebHost.UseUrls("http://localhost:5000");

var app = builder.Build();

try
{
    ShoeStore.api.Data.DbInitializer.Initialize();
    Console.WriteLine("База данных готова");
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка БД: {ex.Message}");
}

app.UseCors("AllowAll");
app.MapControllers();

Console.WriteLine("API запущен на http://localhost:5000");
app.Run();