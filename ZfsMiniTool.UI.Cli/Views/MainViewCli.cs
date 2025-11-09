using ZfsMiniTool.UI.Cli.Utilities;
using ZfsMiniTool.Core.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace ZfsMiniTool.UI.Cli.Views;
public class MainViewCli {
	private MainViewModel dataContext;
	private bool isRunning = true;

	List<CliMenuItem> cliMenuItems;

	public MainViewCli(MainViewModel mainViewModel) {
		dataContext = mainViewModel;

		cliMenuItems = new List<CliMenuItem>() {
		new (){MenuSwitch ="1",  Description = "Importierbare Pools auflisten", ItemAction = ShowSimpleImportablePoolsList },
		new (){MenuSwitch ="2",  Description = "Importierbare Pools in Verzeichnis auflisten", ItemAction = ShowSimpleImportablePoolsListFromdirectory },
		new (){MenuSwitch ="3",  Description = "Keyfile laden",ItemAction = LoadKeyFromFileOperation },
		new (){MenuSwitch ="4",  Description = "Key eingeben",ItemAction = LoadKeyFromCliOperation },
		new (){MenuSwitch ="5",  Description = "Pool importieren",ItemAction = ImportPoolOperation },
		new (){MenuSwitch ="6",  Description = "Pool aus Datei importieren",ItemAction = ImportPoolFromFile },
		new (){MenuSwitch ="7",  Description = "Pool exportieren",ItemAction = ExportPool },
		new (){MenuSwitch ="8",  Description = "Drive-Letter setzen",ItemAction = SetDriveLetterOperation },
		new (){MenuSwitch ="9",  Description = "Dataset mounten",ItemAction = MountDatasetOperation },
		new (){MenuSwitch ="10",  Description = "Datasets auflisten",ItemAction = ListDatasetsOperation},
		new (){MenuSwitch ="q", Description = "Beenden" , ItemAction = QuitApplication},
		};
	}

	private void ExportPool() {
		CliWhileItem cliWhileItem = new();
		cliWhileItem.WhileYesTry(() => {
			Console.WriteLine("Pool name eingeben: ");
			var poolName = Console.ReadLine();

			if (string.IsNullOrWhiteSpace(poolName)) {
				Console.WriteLine("Pool-Name darf nicht leer sein.");
				return;
			}

			dataContext.ExportPool(poolName);
		});
	}

	private void ImportPoolFromFile() {
		CliWhileItem cliWhile = new();
		cliWhile.WhileYesTry(() => {
			Console.Write("Pool-Name eingeben: ");
			var poolName = Console.ReadLine();

			if (string.IsNullOrWhiteSpace(poolName)) {
				Console.WriteLine("Pool-Name darf nicht leer sein.");
				return;
			}

			Console.Write("Verzeichnis eingeben: ");
			var directory = Console.ReadLine();

			if (string.IsNullOrWhiteSpace(directory)) {
				Console.WriteLine("Verzeichnis darf nicht leer sein");
				return;
			};

			directory = directory.Replace("\"", "");

			if (!Directory.Exists(directory)) {
				Console.WriteLine("Verzeichnis existiert nicht.");
				return;
			}

			Console.WriteLine($"Importiere Pool '{poolName}' von {directory}");
			dataContext.ImportPoolFromFile(poolName, directory);
			Console.WriteLine("Pool erfolgreich importiert!");
		});
	}

	private void QuitApplication() {
		isRunning = false;
	}

	public void Show() {
		Console.WriteLine("=== ZFS Mini Tool (Administrator) ===");
		Console.WriteLine();

		while (isRunning) {
			ShowMenu();
			var choice = Console.ReadLine();

			try {
				if (cliMenuItems.FirstOrDefault(entry => entry.MenuSwitch.Equals(choice)) is CliMenuItem menuItem)
					menuItem.ItemAction();

				else
					Console.WriteLine("Input not recognized, please try again...");
			}
			catch (Exception ex) {
				Console.Error.WriteLine($"Fehler: {ex.Message}");
			}
		}
	}

	private void ShowMenu() {
		Console.WriteLine("Available options:\n");
		foreach (var menuItem in cliMenuItems) {
			Console.WriteLine($"{menuItem.MenuSwitch,5}: {menuItem.Description}\n");
		}
		Console.WriteLine();
	}

	private void ImportPoolOperation()
	{
		Console.Write("Pool-Name eingeben: ");
		var poolName = Console.ReadLine();

		if (string.IsNullOrWhiteSpace(poolName))
		{
			Console.WriteLine("Pool-Name darf nicht leer sein.");
			return;
		}

		Console.WriteLine($"Importiere Pool '{poolName}' …");
		dataContext.ImportPool(poolName);
		Console.WriteLine("Pool erfolgreich importiert!");
	}

	private void LoadKeyFromFileOperation() {
		Console.Write("Pool-Name eingeben: ");
		var poolName = Console.ReadLine();

		if (string.IsNullOrWhiteSpace(poolName)) {
			Console.WriteLine("Pool-Name darf nicht leer sein.");
			return;
		}

		Console.Write("Pfad zur Schlüsseldatei eingeben: ");
		var keyPath = Console.ReadLine();

		if (string.IsNullOrWhiteSpace(keyPath)) {
			Console.WriteLine("Schlüsseldatei-Pfad darf nicht leer sein.");
			return;
		}

		keyPath.Replace("\"", ""); // removes quotes, if there are any

        Console.WriteLine("Lade Schlüssel …");
		dataContext.LoadKey(poolName, keyPath);
		Console.WriteLine("Schlüssel erfolgreich geladen!");
	}

	private void LoadKeyFromCliOperation()
	{
			Console.Write("Pool-Name eingeben: ");
		var poolName = Console.ReadLine();
		if (string.IsNullOrWhiteSpace(poolName)) {
			Console.WriteLine("Pool-Name darf nicht leer sein.");
			return;
		}
		Console.Write("Geben Sie den Schlüssel ein: ");
		var key = Console.ReadLine();
		if (string.IsNullOrWhiteSpace(key)) {
			Console.WriteLine("Schlüssel darf nicht leer sein.");
			return;
		}
		Console.WriteLine("Lade Schlüssel …");
		dataContext.LoadKeyFromString(poolName, key);
		Console.WriteLine("Schlüssel erfolgreich geladen!");
    }

	private void SetDriveLetterOperation() {
		Console.Write("Dataset-Name eingeben: ");
		var datasetName = Console.ReadLine();

		if (string.IsNullOrWhiteSpace(datasetName)) {
			Console.WriteLine("Dataset-Name darf nicht leer sein.");
			return;
		}

		Console.WriteLine();
		Console.WriteLine("Drive-Letter Optionen:");
		Console.WriteLine("1. Spezifischen Drive-Letter setzen");
		Console.WriteLine("2. Automatisch zuweisen lassen");
		Console.WriteLine("3. Drive-Letter deaktivieren");
		Console.Write("Ihre Wahl: ");

		var choice = Console.ReadLine();
		Console.WriteLine();

		switch (choice) {
			case "1":
				SetSpecificDriveLetter(datasetName);
				break;
			case "2":
				SetAutoDriveLetter(datasetName);
				break;
			case "3":
				DisableDriveLetterForDataset(datasetName);
				break;
			default:
				Console.WriteLine("Ungültige Auswahl.");
				break;
		}
	}

	private void SetSpecificDriveLetter(string datasetName) {
		Console.Write("Drive-Letter eingeben (z.B. E): ");
		var driveLetterInput = Console.ReadLine();

		if (string.IsNullOrWhiteSpace(driveLetterInput) || driveLetterInput.Length != 1) {
			Console.WriteLine("Bitte geben Sie einen gültigen Drive-Letter ein.");
			return;
		}

		var driveLetter = char.ToUpper(driveLetterInput[0]);

		Console.WriteLine($"Setze Drive-Letter '{driveLetter}' für Dataset '{datasetName}' …");
		dataContext.SetDriveLetter(datasetName, driveLetter);
		Console.WriteLine($"Drive-Letter '{driveLetter}' erfolgreich gesetzt!");
	}

	private void SetAutoDriveLetter(string datasetName) {
		Console.WriteLine($"Aktiviere automatische Drive-Letter-Zuweisung für Dataset '{datasetName}' …");
		dataContext.SetDriveLetterAuto(datasetName);
		Console.WriteLine("Automatische Drive-Letter-Zuweisung erfolgreich aktiviert!");
	}

	private void DisableDriveLetterForDataset(string datasetName) {
		Console.WriteLine($"Deaktiviere Drive-Letter für Dataset '{datasetName}' …");
		dataContext.DisableDriveLetter(datasetName);
		Console.WriteLine("Drive-Letter erfolgreich deaktiviert!");
	}

	private void MountDatasetOperation() {
		Console.Write("Dataset-Name eingeben: ");
		var datasetName = Console.ReadLine();

		if (string.IsNullOrWhiteSpace(datasetName)) {
			Console.WriteLine("Dataset-Name darf nicht leer sein.");
			return;
		}

		Console.WriteLine("Mounten …");
		dataContext.MountDataset(datasetName);
		Console.WriteLine("Dataset erfolgreich gemountet!");
	}

	private void ListDatasetsOperation() {
		Console.Write("Pool-Name eingeben: ");
		var poolName = Console.ReadLine();

		if (string.IsNullOrWhiteSpace(poolName)) {
			Console.WriteLine("Pool-Name darf nicht leer sein.");
			return;
		}

		ShowSimpleDatasetList(poolName);
	}

	private void ShowSimpleDatasetList(string poolName) {
		Console.WriteLine($"Datasets im Pool '{poolName}':");
		Console.WriteLine(new string('-', 40));

		var datasets = dataContext.ListDatasets(poolName);

		if (datasets.Count == 0) {
			Console.WriteLine("Keine Datasets gefunden.");
		}
		else {
			for (int i = 0; i < datasets.Count; i++) {
				Console.WriteLine($"{i + 1}. {datasets[i]}");
			}
			Console.WriteLine();
			Console.WriteLine($"Insgesamt {datasets.Count} Dataset(s) gefunden.");
		}
	}

	private void ShowSimpleImportablePoolsList() {
		Console.WriteLine("Importierbare Pools:");
		Console.WriteLine(new string('-', 40));

		var pools = dataContext.ListImportablePools();

		if (pools.Count == 0) {
			Console.WriteLine("Keine importierbaren Pools gefunden.");
		}
		else {
			for (int i = 0; i < pools.Count; i++) {
				Console.WriteLine($"{i + 1}. {pools[i]}");
			}
			Console.WriteLine();
			Console.WriteLine($"Insgesamt {pools.Count} Pool(s) verfügbar.");
		}
	}

	private void ShowSimpleImportablePoolsListFromdirectory() {

		Console.WriteLine("Please provice directory:");
		var input = Console.ReadLine();

		if (!Path.Exists(input)) {
			Console.WriteLine("Directory does not exist");
		}
		else {
			var pools = dataContext.ListImportablePoolsFromDirectory(input);

			Console.WriteLine("Importierbare Pools:");
			Console.WriteLine(new string('-', 40));

			if (pools.Count == 0) {
				Console.WriteLine("Keine importierbaren Pools gefunden.");
			}
			else {
				for (int i = 0; i < pools.Count; i++) {
					Console.WriteLine($"{i + 1}. {pools[i]}");
				}
				Console.WriteLine();
				Console.WriteLine($"Insgesamt {pools.Count} Pool(s) verfügbar.");
			}
		}
	}
}
