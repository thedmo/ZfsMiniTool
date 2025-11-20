namespace ZfsMiniTool.Core.Models;

public class ZPoolModel : IDataset
{
    public string Name { get; set; } = string.Empty;
    public List<IDataset> DataSets { get; set; } = [];

    public bool FromFile { get; set; }
    public string Path { get; set; } = string.Empty;

    public bool NeedsKey { get; set; }
    public string KeyPath { get; set; } = string.Empty;

    public ZPoolModel()
    {
        DataSets.Add(this);
    }
}
