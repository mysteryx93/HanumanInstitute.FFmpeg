using System.IO;

namespace HanumanInstitute.FFmpeg.Services;

/// <inheritdoc />
public class FileSystemService : IFileSystemService
{
    /// <inheritdoc />
    public bool Exists(string path) => File.Exists(path);

    /// <inheritdoc />
    public void Delete(string path) => File.Delete(path);

    /// <inheritdoc />
    public string GetFileNameWithoutExtension(string path) => Path.GetFileNameWithoutExtension(path);

    /// <inheritdoc />
    public bool IsPathRooted(string path) => Path.IsPathRooted(path);

    /// <inheritdoc />
    public string Combine(string path1, string path2) => Path.Combine(path1, path2);

    /// <inheritdoc />
    public string? GetDirectoryName(string path) => Path.GetDirectoryName(path);

    /// <inheritdoc />
    public string GetTempFile() => Path.GetTempFileName();

    /// <inheritdoc />
    public void WriteAllText(string path, string contents) => File.WriteAllText(path, contents);
}
