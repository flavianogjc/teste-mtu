using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Hosting;
using Mtu.Rentals.Infrastructure.Persistence;
using Mtu.Rentals.Infrastructure;
using Mtu.Rentals.Contracts;
using Mtu.Rentals.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

builder.Services.AddProblemDetails();
builder.Services.AddControllers();

var isTesting = builder.Environment.IsEnvironment("Testing");

// DbContext por ambiente
if (isTesting)
{
    // Placeholder; o CustomWebAppFactory dos testes vai sobrescrever
    builder.Services.AddDbContext<AppDbContext>(o =>
    {
        o.UseSqlite("DataSource=:memory:");
    });
}
else
{
    builder.Services.AddDbContext<AppDbContext>(o =>
    {
        o.UseNpgsql(cfg.GetConnectionString("postgres"));
        o.UseSnakeCaseNamingConvention();
    });
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IFileStorage, LocalFileStorage>();
builder.Services.AddBus(cfg);

var app = builder.Build();

if (app.Environment.IsEnvironment("Testing"))
{
    app.UseDeveloperExceptionPage(); // mostra stack trace nos 500 nos testes
}
else
{
    app.UseExceptionHandler();
}


// Auto-migrate / EnsureCreated
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (isTesting)
    {
        // Em testes não aplicamos migrações para evitar PendingModelChangesWarning
        db.Database.EnsureCreated();
    }
    else
    {
        var hasMigrations = db.Database.GetMigrations().Any();
        if (hasMigrations)
            db.Database.Migrate();
        else
            db.Database.EnsureCreated();
    }
}

app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();

// Necessário para WebApplicationFactory<Program> nos testes de integração
public partial class Program { }
