namespace ZfsMiniTool.Core.Models;

public class ZPoolModel
{
    public string Name { get; set; } = string.Empty;

    public bool FromFile { get; set; }
    public string Path { get; set; } = string.Empty;

    public bool NeedsKey { get; set; }
    public string KeyPath { get; set; } = string.Empty;

    public List<DatasetModel> DataSets { get; set; } = [];
}
