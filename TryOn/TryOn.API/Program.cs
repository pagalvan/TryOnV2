using Microsoft.EntityFrameworkCore;
using TryOn.API.Models;
using TryOn.BLL;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policyBuilder =>
    {
        policyBuilder.WithOrigins("http://localhost:3000") // URL de tu aplicación Next.js en desarrollo
                     .AllowAnyMethod()
                     .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<TryOnDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// Registrar solo los servicios que existen y usas
builder.Services.AddScoped<PrendaService>();
builder.Services.AddScoped<InventarioService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseCors("AllowWebApp");

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
