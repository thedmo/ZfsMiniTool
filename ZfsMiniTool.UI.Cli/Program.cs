using ZfsMiniTool.Core.Services;
using ZfsMiniTool.Core.ViewModels;
using ZfsMiniTool.UI.Cli.Configurations;
using ZfsMiniTool.UI.Cli.Views;

namespace ZfsMiniTool.UI.Cli;

internal class Program
{
    static void Main(string[] args)
    {
        ConfigurationService configService = new();
        OpenZfsService openZfsService = new();
        FileSystemService fileSystemService = new();

        MainViewModel mainViewModel = new(openZfsService, fileSystemService);
        MainViewCli view = new(mainViewModel);

        view.Show();
    }
}

