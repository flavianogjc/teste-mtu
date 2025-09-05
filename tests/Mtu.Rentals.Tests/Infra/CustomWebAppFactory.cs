using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mtu.Rentals.Application;
using Mtu.Rentals.Infrastructure.Persistence;

namespace Mtu.Rentals.Tests.Infra;

public sealed class CustomWebAppFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly SqliteConnection _conn = new("DataSource=:memory:");
    private string? _tmpDir;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _conn.Open();
        builder.UseEnvironment("Testing");  // triggers the Sqlite branch in the Program

        builder.ConfigureServices(services =>
        {
            // Remove ALL already registered AppDbContext options (from Program)
            var toRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>))
                .ToList();
            foreach (var d in toRemove) services.Remove(d);

            services.AddDbContext<AppDbContext>(o => o.UseSqlite(_conn));

            // Temporary disk storage
            _tmpDir = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"cnh-tests-{Guid.NewGuid():N}")
            ).FullName;
            services.AddSingleton<IFileStorage>(new TempFileStorage(_tmpDir));

            // Create schema (in Testing you may prefer EnsureCreated)
            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }

    public new void Dispose()
    {
        base.Dispose();
        _conn.Dispose();

        if (!string.IsNullOrWhiteSpace(_tmpDir) && Directory.Exists(_tmpDir))
        {
            try { Directory.Delete(_tmpDir, recursive: true); } catch { /* ignore */ }
        }
    }

    private sealed class TempFileStorage : IFileStorage
    {
        private readonly string _root;
        public TempFileStorage(string root) => _root = root;

        public async Task<string> SaveAsync(Stream content, string filename, CancellationToken ct)
        {
            Directory.CreateDirectory(_root);
            var path = Path.Combine(_root, filename);
            using var fs = File.Create(path);
            await content.CopyToAsync(fs, ct);
            return path;
        }
    }
}
