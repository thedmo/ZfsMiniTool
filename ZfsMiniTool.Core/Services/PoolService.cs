using ZfsMiniTool.Core.Models;

namespace ZfsMiniTool.Core.Services;

public class PoolService
{
    FileSystemService fileSystemService;

    // Todo: Interface this service and fileSystemService for testing/mocking
    public PoolService(FileSystemService fileSystemService)
    {
        this.fileSystemService = fileSystemService;
    }

    /// <summary>
    /// Listet alle importierbaren Pools auf.
    /// </summary>
    /// <returns>Liste der importierbaren Pool-Namen</returns>
    public List<ZPoolModel> ListImportablePools()
    {
        // -o name gibt nur die Pool-Namen aus
        string output = CommandLineRunner.Run("zpool", "import -o name");
        return GetPoolsFromOutput(output);
    }

    public List<ZPoolModel> ListImportablePoolsFromDirectory(string directory)
    {
        string output = CommandLineRunner.Run("zpool", $"import -d {directory}");

        SetPathsForPools(GetPoolsFromOutput(output), directory);

        return GetPoolsFromOutput(output);
    }

    private List<ZPoolModel> GetPoolsFromOutput(string output)
    {
        var pools = new List<ZPoolModel>();
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var poolName = line.Trim();

            if (string.IsNullOrEmpty(poolName)
                || poolName.StartsWith("[ERR]")
                || !poolName.StartsWith("pool: "))
                continue;

            if (!pools.Any(zPool => zPool.Name == poolName))
                pools.Add(new ZPoolModel
                {
                    Name = poolName,
                    FromFile = false
                });
        }

        return pools;
    }

    private void SetPathsForPools(List<ZPoolModel> pools, string directory)
    {
        foreach (var pool in pools)
        {
            // Todo: Test this with various file extensions
            var foundFiles = fileSystemService.GetFilesFromDirectory(directory, ".*");
            
            var foundPath = foundFiles
                .Where(filePath => Path.GetFileNameWithoutExtension(filePath) == pool.Name)
                .FirstOrDefault();

            if (foundPath is null)
                continue;

            pool.Path = foundPath;
            pool.FromFile = true;
        }
    }

    public void ExportPool(string pool)
    {
        CommandLineRunner.Run("zpool", $"export {pool}");
    }

    public bool TryImportPool(ref ZPoolModel pool, out string errMsg)
    {
        errMsg = string.Empty;
        var output = CommandLineRunner.Run("zpool", $"import {pool.Name}");


        // Todo: Parse output for errors -> set flag in object for key if needed



        return true;
    }

    public bool TryImportPoolFromFile(ZPoolModel pool, out string errMsg)
    {
        errMsg = string.Empty;
        var output = CommandLineRunner.Run("zpool", $"import -d {pool.Path} {pool.Name}");

        // Todo: Parse output for errors -> set flag in object for key if needed


        return true;
    }
}
