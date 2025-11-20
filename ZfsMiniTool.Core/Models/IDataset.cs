namespace ZfsMiniTool.Core.Models;

public interface IDataset
{
    string Name { get; set; }
    List<IDataset> DataSets { get; set; }
}