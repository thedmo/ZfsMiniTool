namespace ZfsMiniTool.UI.Cli.Utilities;
public class CliMenuItem
{
    public required string MenuSwitch { get; set; }
    public required string Description { get; set; }
    public required Action ItemAction { get; set; }
}
