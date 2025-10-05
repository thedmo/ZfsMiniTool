using ZfsMiniTool.Core.Services;
using ZfsMiniTool.Core.Utilities;
using ZfsMiniTool.Core.ViewModels;

namespace ZfsMiniTool.Core.Views;
internal class MainViewCli
{
    private MainViewModel dataContext;
    private ConfigurationService configuration;
    private bool isRunning = true;

    List<CliMenuItem> cliMenuItems;

    public MainViewCli(MainViewModel mainViewModel, ConfigurationService config)
    {
        dataContext = mainViewModel;
        configuration = config;

        cliMenuItems = new List<CliMenuItem>() {
        new (){MenuSwitch ="1",  Description = "Importierbare Pools auflisten", ItemAction = ShowSimpleImportablePoolsList },
        new (){MenuSwitch ="2",  Description = "Importierbare Pools in Verzeichnis auflisten", ItemAction = ShowSimpleImportablePoolsListFromdirectory },
        new (){MenuSwitch ="3",  Description = "Pool importieren",ItemAction = ImportPoolOperation },
        new (){MenuSwitch ="4",  Description = "Schlüssel laden",ItemAction = ImportHexKeyOperation },
        new (){MenuSwitch ="5",  Description = "Drive-Letter setzen",ItemAction = SetDriveLetterOperation },
        new (){MenuSwitch ="6",  Description = "Dataset mounten",ItemAction = MountDatasetOperation },
        new (){MenuSwitch ="7",  Description = "Datasets auflisten",ItemAction = ListDatasetsOperation},
        new (){MenuSwitch ="8",  Description = "Vollständige Sequenz ausführen",ItemAction = RunFullSequence},
        new (){MenuSwitch ="9",  Description = "Hex Key zu raw binary file",ItemAction = ImportHexKeyOperation},
        new (){MenuSwitch ="10", Description = "List key that can be loaded",ItemAction = ListLoadableKeysOperation },
        new (){MenuSwitch ="11", Description = "Beenden" , ItemAction = QuitApplication},
        };
    }

    private void QuitApplication()
    {
        isRunning = false;
    }

    public void Show()
    {
        Console.WriteLine("=== ZFS Mini Tool (Administrator) ===");
        Console.WriteLine();

        while (isRunning)
        {
            ShowMenu();
            var choice = Console.ReadLine();

            try
            {
                if (cliMenuItems.FirstOrDefault(entry => entry.MenuSwitch.Equals(choice)) is CliMenuItem menuItem)
                    menuItem.ItemAction();

                else
                    Console.WriteLine("Input not recognized, please try again...");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Fehler: {ex.Message}");
            }
        }
    }

    private void ShowMenu()
    {
        Console.WriteLine("Available options:\n");
        foreach (var menuItem in cliMenuItems)
        {
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

    private void ImportHexKeyOperation()
    {
        CliWhileItem cliWhileItem = new();

        cliWhileItem.WhileYesTry(() =>
        {
            Console.WriteLine("Import key to store it as a binary file. Please provide your 64 characters long hex key: ");
            string input = Console.ReadLine() ?? throw new ArgumentException("Key cannot be empty");

            Console.WriteLine("provide a directory to store the key in (leave empty for default: C:/Users/<Username>/.zkeys/ ):");
            string? path = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(path))
                path = configuration.DefaultKeyDirectory;

            Console.WriteLine("provide a filename for the keyfile (only name, no extension, default: date + guid)");
            string filename = Console.ReadLine() ?? $"{DateTime.Now.ToShortDateString()} {Guid.NewGuid().ToString()}";

            dataContext.StoreHexKeyAsBinaryFile(path, filename, input);
        });
    }

    private void ListLoadableKeysOperation()
    {
        CliWhileItem cliWhileItem = new();

        cliWhileItem.WhileYesTry(() =>
        {
            Console.WriteLine("provide a path to look for keys (leave empty for default: C:/Users/<Username>/.zkeys/ ):");
            string? directory = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(directory))
                directory = configuration.DefaultKeyDirectory;

            if (string.IsNullOrEmpty(directory))
                throw new ArgumentException($"Path was not valid: {directory}");

            var list = dataContext.GetKeyFilesFrom(directory);

            foreach (var file in list)
            {
                Console.WriteLine(file);
            }
        });
    }

    private void LoadKeyOperation()
    {
        Console.Write("Pool-Name eingeben: ");
        var poolName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(poolName))
        {
            Console.WriteLine("Pool-Name darf nicht leer sein.");
            return;
        }

        Console.Write("Pfad zur Schlüsseldatei eingeben: ");
        var keyPath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(keyPath))
        {
            Console.WriteLine("Schlüsseldatei-Pfad darf nicht leer sein.");
            return;
        }

        Console.WriteLine("Lade Schlüssel …");
        dataContext.LoadKey(poolName, keyPath);
        Console.WriteLine("Schlüssel erfolgreich geladen!");
    }

    private void SetDriveLetterOperation()
    {
        Console.Write("Dataset-Name eingeben: ");
        var datasetName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(datasetName))
        {
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

        switch (choice)
        {
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

    private void SetSpecificDriveLetter(string datasetName)
    {
        Console.Write("Drive-Letter eingeben (z.B. E): ");
        var driveLetterInput = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(driveLetterInput) || driveLetterInput.Length != 1)
        {
            Console.WriteLine("Bitte geben Sie einen gültigen Drive-Letter ein.");
            return;
        }

        var driveLetter = char.ToUpper(driveLetterInput[0]);

        Console.WriteLine($"Setze Drive-Letter '{driveLetter}' für Dataset '{datasetName}' …");
        dataContext.SetDriveLetter(datasetName, driveLetter);
        Console.WriteLine($"Drive-Letter '{driveLetter}' erfolgreich gesetzt!");
    }

    private void SetAutoDriveLetter(string datasetName)
    {
        Console.WriteLine($"Aktiviere automatische Drive-Letter-Zuweisung für Dataset '{datasetName}' …");
        dataContext.SetDriveLetterAuto(datasetName);
        Console.WriteLine("Automatische Drive-Letter-Zuweisung erfolgreich aktiviert!");
    }

    private void DisableDriveLetterForDataset(string datasetName)
    {
        Console.WriteLine($"Deaktiviere Drive-Letter für Dataset '{datasetName}' …");
        dataContext.DisableDriveLetter(datasetName);
        Console.WriteLine("Drive-Letter erfolgreich deaktiviert!");
    }

    private void MountDatasetOperation()
    {
        Console.Write("Dataset-Name eingeben: ");
        var datasetName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(datasetName))
        {
            Console.WriteLine("Dataset-Name darf nicht leer sein.");
            return;
        }

        Console.WriteLine("Mounten …");
        dataContext.MountDataset(datasetName);
        Console.WriteLine("Dataset erfolgreich gemountet!");
    }

    private void ListDatasetsOperation()
    {
        Console.Write("Pool-Name eingeben: ");
        var poolName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(poolName))
        {
            Console.WriteLine("Pool-Name darf nicht leer sein.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Wählen Sie die Anzeigeart:");
        Console.WriteLine("1. Einfache Liste (nur Namen)");
        Console.WriteLine("2. Detaillierte Informationen");
        Console.Write("Ihre Wahl: ");

        var choice = Console.ReadLine();
        Console.WriteLine();

        switch (choice)
        {
            case "1":
                ShowSimpleDatasetList(poolName);
                break;
            case "2":
                ShowDetailedDatasetList(poolName);
                break;
            default:
                Console.WriteLine("Ungültige Auswahl, zeige einfache Liste:");
                ShowSimpleDatasetList(poolName);
                break;
        }
    }

    private void ShowSimpleDatasetList(string poolName)
    {
        Console.WriteLine($"Datasets im Pool '{poolName}':");
        Console.WriteLine(new string('-', 40));

        var datasets = dataContext.ListDatasets(poolName);

        if (datasets.Count == 0)
        {
            Console.WriteLine("Keine Datasets gefunden.");
        }
        else
        {
            for (int i = 0; i < datasets.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {datasets[i]}");
            }
            Console.WriteLine();
            Console.WriteLine($"Insgesamt {datasets.Count} Dataset(s) gefunden.");
        }
    }

    private void ShowDetailedDatasetList(string poolName)
    {
        Console.WriteLine($"Detaillierte Dataset-Informationen für Pool '{poolName}':");
        Console.WriteLine(new string('=', 60));

        var detailedOutput = dataContext.ListDatasetsDetailed(poolName);
        Console.WriteLine(detailedOutput);
    }

    private void RunFullSequence()
    {
        Console.Write("Pool-Name eingeben: ");
        var poolName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(poolName))
        {
            Console.WriteLine("Pool-Name darf nicht leer sein.");
            return;
        }

        Console.Write("Pfad zur Schlüsseldatei eingeben (Enter für überspringen): ");
        var keyPath = Console.ReadLine();

        Console.Write("Drive-Letter eingeben (z.B. E, oder Enter für automatische Zuweisung): ");
        var driveLetterInput = Console.ReadLine();

        Console.WriteLine("Führe vollständige Sequenz aus...");
        Console.WriteLine();

        // 1. Pool importieren
        Console.WriteLine($"1. Importiere Pool '{poolName}' …");
        dataContext.ImportPool(poolName);

        // 2. Schlüssel laden (falls angegeben)
        if (!string.IsNullOrWhiteSpace(keyPath))
        {
            Console.WriteLine("2. Lade Schlüssel …");
            dataContext.LoadKey(poolName, keyPath);
        }

        // 3. Drive-Letter setzen
        Console.WriteLine("3. Setze Drive‑Letter‑Property …");
        if (string.IsNullOrWhiteSpace(driveLetterInput))
        {
            dataContext.SetDriveLetterAuto(poolName);
            Console.WriteLine("Automatische Drive-Letter-Zuweisung aktiviert.");
        }
        else if (driveLetterInput.Length == 1)
        {
            var driveLetter = char.ToUpper(driveLetterInput[0]);
            dataContext.SetDriveLetter(poolName, driveLetter);
            Console.WriteLine($"Drive-Letter '{driveLetter}' gesetzt.");
        }
        else
        {
            Console.WriteLine("Ungültiger Drive-Letter, verwende automatische Zuweisung.");
            dataContext.SetDriveLetterAuto(poolName);
        }

        // 4. Dataset mounten
        Console.WriteLine("4. Mounten …");
        dataContext.MountDataset(poolName);

        Console.WriteLine("Vollständige Sequenz erfolgreich abgeschlossen!");
    }

    private void ShowSimpleImportablePoolsList()
    {
        Console.WriteLine("Importierbare Pools:");
        Console.WriteLine(new string('-', 40));

        var pools = dataContext.ListImportablePools();

        if (pools.Count == 0)
        {
            Console.WriteLine("Keine importierbaren Pools gefunden.");
        }
        else
        {
            for (int i = 0; i < pools.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {pools[i]}");
            }
            Console.WriteLine();
            Console.WriteLine($"Insgesamt {pools.Count} Pool(s) verfügbar.");
        }
    }

    private void ShowSimpleImportablePoolsListFromdirectory()
    {

        Console.WriteLine("Please provice directory:");
        var input = Console.ReadLine();

        if (!Path.Exists(input))
        {
            Console.WriteLine("Directory does not exist");
        }
        else
        {
            var pools = dataContext.ListImportablePoolsFromDirectory(input);

            Console.WriteLine("Importierbare Pools:");
            Console.WriteLine(new string('-', 40));

            if (pools.Count == 0)
            {
                Console.WriteLine("Keine importierbaren Pools gefunden.");
            }
            else
            {
                for (int i = 0; i < pools.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {pools[i]}");
                }
                Console.WriteLine();
                Console.WriteLine($"Insgesamt {pools.Count} Pool(s) verfügbar.");
            }
        }
    }

    private void ShowDetailedImportablePoolsList()
    {
        Console.WriteLine("Detaillierte Informationen über importierbare Pools:");
        Console.WriteLine(new string('=', 60));

        var detailedOutput = dataContext.ListImportablePoolsDetailed();
        Console.WriteLine(detailedOutput);
    }
}
