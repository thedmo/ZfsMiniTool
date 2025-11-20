using System.Diagnostics;
using System.Text;
using ZfsMiniTool.Core.Models;

namespace ZfsMiniTool.Core.Services;

public class OpenZfsService
{

    public void CreateFileBasedPool(string poolName, string directory, long sizeInMB)
    {
        // Create a sparse file of the specified size
        using (var fs = new FileStream(directory, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            fs.SetLength(sizeInMB * 1024 * 1024);
        }

        var fullPath = Path.Combine(directory, poolName, ".zdev");

        CommandLineRunner.Run("fsutil", $"{fullPath}");

        // Create the ZFS pool using the sparse file
        CommandLineRunner.Run("zpool", @$"create {poolName} \\?\{fullPath}");
    }

    public void ExportPool(string pool)
    {
        CommandLineRunner.Run("zpool", $"export {pool}");
    }

    public void ImportPool(string pool)
    {
        CommandLineRunner.Run("zpool", $"import {pool}");
    }

    public void ImportPoolFromFile(string poolName, string path)
    {
        CommandLineRunner.Run("zpool", $"import -d {path} {poolName}");
    }

    public void LoadKey(string pool, string keyFile)
    {
        // Read the key from the file
        if (!File.Exists(keyFile))
            throw new FileNotFoundException($"Key file not found: {keyFile}");

        string key = File.ReadAllText(keyFile).Trim();

        LoadKeyFromString(pool, key);
    }

    public void LoadKeyFromString(string pool, string key)
    {
        CommandLineRunner.Run("zfs", $"load-key {pool}", key);
    }

    public void SetDriveLetter(string dataset, bool on, char? letter = null)
    {
        string value;
        if (on)
        {
            if (letter.HasValue)
            {
                value = letter.Value.ToString().ToUpper();
            }
            else
            {
                // If on=true but no letter specified, let ZFS auto-assign
                value = "on";
            }
        }
        else
        {
            value = "off";
        }

        CommandLineRunner.Run("zfs", $"set driveletter={value} {dataset}");
    }

    public void MountDataset(string dataset, string? mountPoint = null)
    {
        // Ohne mountpoint-Argument wird das standardmäßige Drive‑Letter‑Verhalten verwendet.
        string args = mountPoint == null ? $"{dataset}" : $"{dataset} {mountPoint}";
        CommandLineRunner.Run("zfs", $"mount {args}");
    }

    /// <summary>
    /// Listet alle Datasets eines gegebenen Pools auf.
    /// </summary>
    /// <param name="pool">Der Name des Pools</param>
    /// <returns>Liste der Dataset-Namen</returns>
    public List<string> ListDatasets(string pool)
    {
        // Verwende -H für script-freundliche Ausgabe (keine Header)
        // -o name gibt nur die Namen aus
        // -r für rekursive Auflistung aller Datasets im Pool
        string output = CommandLineRunner.Run("zfs", $"list -H -o name -r {pool}");

        var datasets = new List<string>();
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("[ERR]")
                || string.IsNullOrEmpty(trimmedLine))
                continue;

            datasets.Add(trimmedLine);
        }

        return datasets;
    }

    /// <summary>
    /// Listet alle importierbaren Pools auf.
    /// </summary>
    /// <returns>Liste der importierbaren Pool-Namen</returns>
    public List<string> ListImportablePools()
    {
        // -o name gibt nur die Pool-Namen aus
        string output = CommandLineRunner.Run("zpool", "import -o name");
        return GetPoolsFromOutput(output);
    }

    public List<string> ListImportablePoolsFromDirectory(string directory)
    {
        string output = CommandLineRunner.Run("zpool", $"import -d {directory}");

        return GetPoolsFromOutput(output);
    }

    private List<string> GetPoolsFromOutput(string output)
    {
        var poolStrings = new List<string>();
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var poolName = line.Trim();

            if (string.IsNullOrEmpty(poolName)
                || poolName.StartsWith("[ERR]")
                || !poolName.StartsWith("pool: "))
                continue;

            poolStrings.Add(poolName);
        }

        return poolStrings;
    }
}
