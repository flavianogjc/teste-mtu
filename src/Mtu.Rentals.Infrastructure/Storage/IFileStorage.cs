public interface IFileStorage
{
    Task<string> SaveAsync(Stream stream, string fileName, CancellationToken ct);
}