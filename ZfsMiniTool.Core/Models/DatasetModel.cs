namespace ZfsMiniTool.Core.Models;

public class DatasetModel : IDataset
{
    public string Name { get; set; } = string.Empty;
    public List<IDataset> DataSets { get; set; } = [];
}
