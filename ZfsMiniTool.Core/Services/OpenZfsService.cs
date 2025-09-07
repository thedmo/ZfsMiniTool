using System.Diagnostics;
using System.Text;

namespace ZfsMiniTool.Core.Services;
internal class OpenZfsService
{
    /// <summary>
    /// Führt ein externes Programm aus und gibt stdout + stderr zurück.
    /// </summary>
    public string Run(string exe, string args)
    {
        return Run(exe, args, null);
    }

    /// <summary>
    /// Führt ein externes Programm aus und gibt stdout + stderr zurück.
    /// </summary>
    /// <param name="exe">Executable to run</param>
    /// <param name="args">Arguments for the executable</param>
    /// <param name="input">Optional input to send to the process stdin</param>
    /// <returns>Combined stdout and stderr output</returns>
    public string Run(string exe, string args, string? input)
    {
        var psi = new ProcessStartInfo
        {
            FileName = exe,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = input != null,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = Process.Start(psi);
        var outBuilder = new StringBuilder();

        proc.OutputDataReceived += (s, e) => { if (e.Data != null) outBuilder.AppendLine(e.Data); };
        proc.ErrorDataReceived += (s, e) => { if (e.Data != null) outBuilder.AppendLine("[ERR] " + e.Data); };

        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        // Send input if provided
        if (input != null)
        {
            proc.StandardInput.WriteLine(input);
            proc.StandardInput.Close();
        }

        proc.WaitForExit();

        if (proc.ExitCode != 0)
            throw new InvalidOperationException(
                $"'{exe} {args}' returned exit code {proc.ExitCode}. Output:\n{outBuilder}");

        return outBuilder.ToString();
    }

    // Wrapper‑Methoden für die einzelnen ZFS‑Operationen
    public void ExportPool(string pool) => Run("zpool", $"export {pool}");
    public void ImportPool(string pool) => Run("zpool", $"import {pool}");

    public void LoadKey(string pool, string keyFile)
    {
        // Read the key from the file
        if (!File.Exists(keyFile))
            throw new FileNotFoundException($"Key file not found: {keyFile}");

        string key = File.ReadAllText(keyFile).Trim();

        // Run the command without -L flag and pass key as input
        Run("zfs", $"load-key {pool}", key);
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

        Run("zfs", $"set driveletter={value} {dataset}");
    }

    public void MountDataset(string dataset, string? mountPoint = null)
    {
        // Ohne mountpoint-Argument wird das standardmäßige Drive‑Letter‑Verhalten verwendet.
        string args = mountPoint == null ? $"{dataset}" : $"{dataset} {mountPoint}";
        Run("zfs", $"mount {args}");
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
        string output = Run("zfs", $"list -H -o name -r {pool}");

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
    /// Listet alle Datasets eines Pools mit zusätzlichen Informationen auf.
    /// </summary>
    /// <param name="pool">Der Name des Pools</param>
    /// <returns>Formatierte Ausgabe mit Dataset-Informationen</returns>
    public string ListDatasetsDetailed(string pool)
    {
        // Zeigt Name, Used, Available, Refer, Mountpoint
        return Run("zfs", $"list -r {pool}");
    }

    /// <summary>
    /// Listet alle importierbaren Pools auf.
    /// </summary>
    /// <returns>Liste der importierbaren Pool-Namen</returns>
    public List<string> ListImportablePools()
    {
        // -o name gibt nur die Pool-Namen aus
        string output = Run("zpool", "import -o name");

        var pools = new List<string>();
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrEmpty(trimmedLine)
                || trimmedLine.StartsWith("[ERR]")
                || !trimmedLine.StartsWith("pool: "))
                continue;

            pools.Add(trimmedLine);
        }

        return pools;
    }

    /// <summary>
    /// Listet alle importierbaren Pools mit detaillierten Informationen auf.
    /// </summary>
    /// <returns>Formatierte Ausgabe mit Pool-Informationen</returns>
    public string ListImportablePoolsDetailed()
    {
        // Zeigt detaillierte Informationen über importierbare Pools
        return Run("zpool", "import");
    }
}
