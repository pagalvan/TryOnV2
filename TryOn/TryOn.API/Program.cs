using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TryOn.API.Models;
using TryOn.BLL;
using TryOn.DAL; // Agregar para el repositorio
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ CONFIGURAR BASE DE DATOS (esto es necesario para tu InventarioRepository)
builder.Services.AddDbContext<TryOnDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ REGISTRAR REPOSITORIOS (si los usas)
builder.Services.AddScoped<InventarioRepository>();

// ✅ REGISTRAR SERVICIOS DE NEGOCIO
builder.Services.AddScoped<PrendaService>();
builder.Services.AddScoped<InventarioService>();

// ✅ CONFIGURAR CORS PARA DESARROLLO Y PRODUCCIÓN
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });

    options.AddPolicy("AllowViteDevServer", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });

    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // ✅ USAR CORS PARA DESARROLLO
    app.UseCors("AllowViteDevServer");
}
else
{
    // ✅ USAR CORS PARA PRODUCCIÓN
    app.UseCors("AllowWebApp");
}

app.UseHttpsRedirection();

// ✅ CONFIGURAR ARCHIVOS ESTÁTICOS PARA EL SPA
app.UseDefaultFiles();
app.UseStaticFiles();

// ✅ CONFIGURACIÓN ADICIONAL PARA SERVIR EL FRONTEND VITE
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
    RequestPath = "",
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});

app.UseRouting();
app.UseAuthorization();

// ✅ MAPEAR CONTROLADORES (API)
app.MapControllers();

// ✅ FALLBACK PARA SPA (SOLO SI TIENES FRONTEND EN WWWROOT)
app.MapFallbackToFile("index.html");

Console.WriteLine("🚀 Servidor TryOn iniciado");
Console.WriteLine($"📱 Frontend: {builder.Environment.ContentRootPath}/wwwroot");
Console.WriteLine("🔌 API: /api/inventario");
Console.WriteLine("📋 Swagger: /swagger");

app.Run();
