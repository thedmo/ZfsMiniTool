using Microsoft.Extensions.Configuration;
using ZfsMiniTool.Core.Configurations;

namespace ZfsMiniTool.Core.Services;
internal class ConfigurationService
{
    IConfiguration appConfig;

    public string DefaultKeyDirectory
    {
        get
        {
            var dirConfig = appConfig
                .GetSection("KeyFileStorageConfig")?
                .Get<KeyFileStorageConfig>();

            var path = Path.Combine(dirConfig?.UserHomeDirectory.Replace("%_USER_HOMEDIRECTORY_%", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)),
                  dirConfig?.DirectoryName);

            if (!Path.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }
    }

    public ConfigurationService()
    {
        appConfig = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();
    }
}
