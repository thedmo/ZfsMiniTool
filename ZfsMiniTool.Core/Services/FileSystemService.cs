namespace ZfsMiniTool.Core.Services;
internal class FileSystemService
{
    /// <summary>
    /// Writes binary data under given path to disk
    /// </summary>
    /// <param name="path">path to store file under</param>
    /// <param name="data">data to be stored</param>
    /// <param name="extension">extension to use for this file</param>
    /// <param name="overrideIfExists">force storing new data, even if file already exists</param>
    /// <exception cref="ArgumentNullException">if arguments carry faulty data</exception>
    /// <exception cref="ArgumentException">general error with argument</exception>
    public void StoreToDisk(string path, string filename, byte[] data, string extension, bool overrideIfExists = false)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException("Path cannot be empy");

        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentNullException("filename cannot be empty");

        if (data == null)
            throw new ArgumentNullException("data cannot be empty");

        if (string.IsNullOrWhiteSpace(extension))
            throw new ArgumentNullException("extension not set");

        if (File.Exists(path) && !overrideIfExists)
            throw new ArgumentException("File with same name already exists.");

        var fileWithExtention = Path.ChangeExtension(filename, extension);
        var fullPath = Path.Combine(path, fileWithExtention);

        File.WriteAllBytes(fullPath, data);
    }


    /// <summary>
    /// Retrieves all file paths from a directory that match the specified extension
    /// </summary>
    /// <param name="directory">The directory path to search in</param>
    /// <param name="extension">The file extension to filter by (e.g., ".txt", ".cs")</param>
    /// <returns>An enumerable collection of file paths that match the extension</returns>
    /// <exception cref="ArgumentNullException">Thrown when directory or extension is null</exception>
    /// <exception cref="ArgumentException">Thrown when directory does not exist</exception>
    public IEnumerable<string> GetFilesFromDirectory(string directory, string extension)
    {

        if (directory == null)
            throw new ArgumentNullException("Path cannot be empy");

        if (extension == null)
            throw new ArgumentNullException("extension not set");

        if (!Directory.Exists(directory))
            throw new ArgumentException("Directory not found");

        return Directory.GetFiles(directory, $"*{extension}", SearchOption.AllDirectories);

    }
}
