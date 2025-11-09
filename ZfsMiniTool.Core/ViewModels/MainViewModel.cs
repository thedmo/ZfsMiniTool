using ZfsMiniTool.Core.Services;

namespace ZfsMiniTool.Core.ViewModels;
public class MainViewModel {
	private OpenZfsService openZfsService;
	private FileSystemService fileSystemService;

	public MainViewModel(OpenZfsService openZfsService, FileSystemService fileSystemService) {
		this.openZfsService = openZfsService;
		this.fileSystemService = fileSystemService;
	}

	public void ImportPool(string poolName) {
		openZfsService.ImportPool(poolName);
	}

	public void ImportPoolFromFile(string poolName, string directory) {
		openZfsService.ImportPoolFromFile(poolName, directory);
	}

	public void CreateFileBasedPool(string poolName, string directory, long sizeInMB) {
		openZfsService.CreateFileBasedPool(poolName, directory, sizeInMB);
	}

	public void ExportPool(string poolName) {
		openZfsService.ExportPool(poolName);
	}

	public void LoadKey(string poolName, string keyPath) {
		openZfsService.LoadKey(poolName, keyPath);
	}

	public void LoadKeyFromString(string poolName, string keyString) {
		openZfsService.LoadKeyFromString(poolName, keyString);
    }

    public void SetDriveLetter(string datasetName, char driveLetter) {
		openZfsService.SetDriveLetter(datasetName, true, driveLetter);
	}

	public void SetDriveLetterAuto(string datasetName) {
		openZfsService.SetDriveLetter(datasetName, true);
	}

	public void DisableDriveLetter(string datasetName) {
		openZfsService.SetDriveLetter(datasetName, false);
	}

	public void MountDataset(string datasetName) {
		openZfsService.MountDataset(datasetName);
	}

	public List<string> ListDatasets(string poolName) {
		return openZfsService.ListDatasets(poolName);
	}

	public List<string> ListImportablePools() {
		return openZfsService.ListImportablePools();
	}

	public List<string> ListImportablePoolsFromDirectory(string dir) {
		return openZfsService.ListImportablePoolsFromDirectory(dir);
	}
}
