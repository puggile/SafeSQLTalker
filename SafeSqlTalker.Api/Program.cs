using SafeSqlTalker.Core.Interfaces;
using SafeSqlTalker.Infrastructure.AI;
using SafeSqlTalker.Infrastructure.Configuration;
using SafeSqlTalker.Infrastructure.Data;
using SafeSqlTalker.Infrastructure.Security;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AiSettings>(
    builder.Configuration.GetSection(AiSettings.SectionName));

// Configurazione Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();


// Add services to the container.
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Infrastructure
builder.Services.AddSingleton<DbInitializer>(); // Singleton perché lo usiamo una volta sola all'avvio
builder.Services.AddScoped<ISqlExecutor, SqliteExecutor>();
builder.Services.AddScoped<ISqlGuard, SqlGuard>();

// AI Service
builder.Services.AddScoped<IAiSqlGenerator, SemanticKernelSqlService>();

var app = builder.Build();

// Inizializzazione Database all'avvio
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    initializer.Initialize();
}

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    // Genera il JSON OpenAPI (default: /openapi/v1.json)
    app.MapOpenApi();

    // Attiva l'interfaccia grafica Scalar
    // Sarà disponibile su: http://localhost:5132/scalar/v1
    app.MapScalarApiReference();
    // -----------------------
}

app.UseSerilogRequestLogging();
//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
