namespace TelltaleLocalizationTool.Helper;

public sealed class TempFile : IDisposable
{
    private readonly FileStream lockStream;
    private bool disposed;
    private readonly string normalName;

    public string NormalName => normalName;
    public string Path { get; private set; }


    public TempFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Value cannot be null or whitespace!", nameof(fileName));

        normalName = System.IO.Path.GetFileName(fileName);
        string directory = System.IO.Path.GetTempPath();
        string name = System.IO.Path.GetFileNameWithoutExtension(normalName);
        string extension = System.IO.Path.GetExtension(normalName);

        while (true)
        {
            string candidate = System.IO.Path.Combine(
                directory,
                $"{name}_{Guid.NewGuid():N}{extension}");

            try
            {
                lockStream = new FileStream(
                    candidate,
                    FileMode.CreateNew,
                    FileAccess.ReadWrite,
                    FileShare.Read | FileShare.Write);

                Path = candidate;
                break;
            }
            catch (IOException)
            {
            }
        }
    }

    public void CopyResultTo(string destination)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);

        string target;

        if (Directory.Exists(destination))
        {
            target = System.IO.Path.Combine(destination, NormalName);
        }
        else if (string.IsNullOrEmpty(System.IO.Path.GetExtension(destination)))
        {
            Directory.CreateDirectory(destination);
            target = System.IO.Path.Combine(destination, NormalName);
        }
        else
        {
            string? directory = System.IO.Path.GetDirectoryName(destination);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            target = destination;
        }

        File.Copy(Path, target);
    }

    ~TempFile()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool _)
    {
        if (disposed)
            return;

        disposed = true;

        lockStream.Dispose();

        try
        {
            File.Delete(Path);
        }
        catch
        {
        }
    }
}