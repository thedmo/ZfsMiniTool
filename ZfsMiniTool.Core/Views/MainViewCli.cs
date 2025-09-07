using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZfsMiniTool.Core.Configurations;
using ZfsMiniTool.Core.Services;
using ZfsMiniTool.Core.Utilities;
using ZfsMiniTool.Core.ViewModels;

namespace ZfsMiniTool.Core.Views;
internal class MainViewCli
{
    private MainViewModel dataContext;
    private ConfigurationService configuration;

    public MainViewCli(MainViewModel mainViewModel, ConfigurationService config)
    {
        dataContext = mainViewModel;
        configuration = config;
    }

    public void Show()
    {
        Console.WriteLine("=== ZFS Mini Tool (Administrator) ===");
        Console.WriteLine();

        while (true)
        {
            ShowMenu();
            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        ListImportablePoolsOperation();
                        break;
                    case "2":
                        ImportPoolOperation();
                        break;
                    case "3":
                        LoadKeyOperation();
                        break;
                    case "4":
                        SetDriveLetterOperation();
                        break;
                    case "5":
                        MountDatasetOperation();
                        break;
                    case "6":
                        ListDatasetsOperation();
                        break;
                    case "7":
                        RunFullSequence();
                        break;

                    case "8":
                        ImportHexKeyOperation();
                        break;

                    case "9":
                        ListLoadableKeysOperation();
                        break;

                    case "0":
                        Console.WriteLine("Auf Wiedersehen!");
                        return;
                    default:
                        Console.WriteLine("Ungültige Auswahl. Bitte versuchen Sie es erneut.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Fehler: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Drücken Sie eine beliebige Taste, um fortzufahren...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    private void ShowMenu()
    {
        Console.WriteLine("Verfügbare Optionen:");
        Console.WriteLine("1. Importierbare Pools auflisten");
        Console.WriteLine("2. Pool importieren");
        Console.WriteLine("3. Schlüssel laden");
        Console.WriteLine("4. Drive-Letter setzen");
        Console.WriteLine("5. Dataset mounten");
        Console.WriteLine("6. Datasets auflisten");
        Console.WriteLine("7. Vollständige Sequenz ausführen");
        Console.WriteLine("8. Hex Key zu raw binary file");
        Console.WriteLine("9. List key that can be loaded");
        Console.WriteLine("0. Beenden");
        Console.WriteLine();
        Console.Write("Ihre Wahl: ");
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
        CliWhileItem cliWhileItem = new ();

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

            foreach ( var file in list)
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

    private void ListImportablePoolsOperation()
    {
        Console.WriteLine();
        Console.WriteLine("Wählen Sie die Anzeigeart:");
        Console.WriteLine("1. Einfache Liste (nur Pool-Namen)");
        Console.WriteLine("2. Detaillierte Informationen");
        Console.Write("Ihre Wahl: ");

        var choice = Console.ReadLine();
        Console.WriteLine();

        switch (choice)
        {
            case "1":
                ShowSimpleImportablePoolsList();
                break;
            case "2":
                ShowDetailedImportablePoolsList();
                break;
            default:
                Console.WriteLine("Ungültige Auswahl, zeige einfache Liste:");
                ShowSimpleImportablePoolsList();
                break;
        }
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

    private void ShowDetailedImportablePoolsList()
    {
        Console.WriteLine("Detaillierte Informationen über importierbare Pools:");
        Console.WriteLine(new string('=', 60));

        var detailedOutput = dataContext.ListImportablePoolsDetailed();
        Console.WriteLine(detailedOutput);
    }
}
