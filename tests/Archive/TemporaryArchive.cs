namespace IbkrToEtax.Tests.Archive;

internal sealed class TemporaryArchive : IDisposable
{
    public TemporaryArchive()
    {
        OutputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"ibkr-to-etax-archive-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(OutputDirectory);
    }

    public string OutputDirectory { get; }

    public string AddFile(string name, DateTime modifiedAtUtc, string content = "test")
    {
        var path = Path.Combine(OutputDirectory, name);
        File.WriteAllText(path, content);
        File.SetLastWriteTimeUtc(path, modifiedAtUtc);
        return path;
    }

    public void Dispose()
    {
        if (Directory.Exists(OutputDirectory))
        {
            Directory.Delete(OutputDirectory, recursive: true);
        }
    }
}
