using System.Diagnostics;
using System.Text;

namespace ZfsMiniTool.Core.Services;

public class CommandLineRunner
{
    /// <summary>
    /// Führt ein externes Programm aus und gibt stdout + stderr zurück.
    /// </summary>
    public static string Run(string exe, string args)
    {
        return Run(exe, args, null);
    }

    /// <summary>
    /// Führt ein externes Programm aus und gibt stdout + stderr zurück.
    /// </summary>
    /// <param name="executablePath">Executable to run</param>
    /// <param name="args">Arguments for the executable</param>
    /// <param name="input">Optional input to send to the process stdin</param>
    /// <returns>Combined stdout and stderr output</returns>
    public static string Run(string executablePath, string args, string? input)
    {
        var psi = new ProcessStartInfo
        {
            FileName = executablePath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = input != null, // true if we have input to send
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = Process.Start(psi);
        var outBuilder = new StringBuilder();

        if (proc == null)
            throw new InvalidOperationException($"Failed to start process: {executablePath} {args}");

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
                $"'{executablePath} {args}' returned exit code {proc.ExitCode}. Output:\n{outBuilder}");

        return outBuilder.ToString();
    }
}
