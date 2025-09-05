using Microsoft.Extensions.Configuration;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _root;
    public LocalFileStorage(IConfiguration cfg)
        => _root = Path.GetFullPath(cfg["Storage:Root"] ?? "./storage");

    public async Task<string> SaveAsync(Stream s, string fileName, CancellationToken ct)
    {
        Directory.CreateDirectory(_root);
        var path = Path.Combine(_root, fileName);
        await using var fs = File.Create(path);
        await s.CopyToAsync(fs, ct);
        return path;
    }
}