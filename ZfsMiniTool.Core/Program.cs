using ZfsMiniTool.Core.Services;
using ZfsMiniTool.Core.ViewModels;
using ZfsMiniTool.Core.Views;

namespace ZfsMiniTool.Core
{
    internal class Program
    {
        static void Main(string[] args)
        {

            ConfigurationService configService = new ConfigurationService();
            OpenZfsService openZfsService = new OpenZfsService();
            FileSystemService fileSystemService = new FileSystemService();

            MainViewModel mainViewModel = new MainViewModel(openZfsService, fileSystemService);
            MainViewCli view = new MainViewCli(mainViewModel, configService);

            view.Show();
        }
    }
}
