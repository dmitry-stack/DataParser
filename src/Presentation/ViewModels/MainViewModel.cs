using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProcessingApp.Application.DTOs;
using ProcessingApp.Application.Interfaces;
using ProcessingApp.Presentation.Localization;
using Serilog;
using System.Collections.ObjectModel;
using System.Windows;

namespace ProcessingApp.Presentation.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IRecordRepository _repository;

    private readonly ICsvImporter _csvImporter;

    private readonly IEnumerable<IExporter> _exporters;

    public MainViewModel(
        IRecordRepository repository,
       IEnumerable<IExporter> exporters,
        ICsvImporter csvImporter)
    {
        _repository = repository;
        _exporters = exporters;
        _csvImporter = csvImporter;
    }

    [ObservableProperty]
    private string? _firstName;

    [ObservableProperty]
    private string? _lastName;
    [ObservableProperty]
    private string? _surName;
    [ObservableProperty]
    private string? _city;
    [ObservableProperty]
    private string? _country;
    [ObservableProperty]
    private DateTime? _date;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 50;

    [ObservableProperty]
    private ObservableCollection<RecordDTO> _records = new();

    private async IAsyncEnumerable<RecordDTO> GetRecordsAsync()
    {
        foreach (var record in Records)
        {
            yield return record;
            await Task.Yield();
        }
    }

    [RelayCommand]
    private void SetLanguage(string cultureCode)
    {
        Strings.Instance.SetLanguage(cultureCode == "en" ? AppLanguage.English : AppLanguage.Russian);
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            Records.Clear();

            var result = _repository.GetFilteredRecordsAsync(
                Date, FirstName, SurName, City, Country, LastName, CurrentPage, PageSize);

            await foreach (var record in result)
            {
                Records.Add(record);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при поиске записей в БД.");
            System.Windows.MessageBox.Show("Не удалось выполнить поиск.", "Ошибка БД", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task ImportCsvAsync(CancellationToken token)
    {
        var dialog = new OpenFileDialog { Filter = "CSV Files (*.csv)|*.csv" };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var culture = new System.Globalization.CultureInfo("ru-RU");
                await _csvImporter.ImportCsvAsync(dialog.FileName, culture);

                MessageBox.Show(
                    Strings.Instance.ImportSuccessMessage,
                    Strings.Instance.ImportSuccessTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await SearchAsync();
            }
            catch (OperationCanceledException)
            {
                Log.Information("Импорт CSV был отменен пользователем.");
                MessageBox.Show(
                    Strings.Instance.ImportCancelledMessage,
                    Strings.Instance.ImportCancelledTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при импорте файла: {FileName}", dialog.FileName);
                MessageBox.Show(
                    Strings.Instance.ImportErrorMessage,
                    Strings.Instance.ImportErrorTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private async Task ExportExcelAsync()
    {
        var dialog = new SaveFileDialog { Filter = "Excel Files (*.xlsx)|*.xlsx", DefaultExt = ".xlsx" };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var exporter = _exporters.FirstOrDefault(e => e.SupportedExtension == ".xlsx");

                if (exporter == null)
                {
                    System.Windows.MessageBox.Show("В системе не зарегистрирован модуль для экспорта в Excel.", "Ошибка конфигурации");
                    return;
                }

                await exporter.ExportAsync(GetRecordsAsync(), dialog.FileName);
                System.Windows.MessageBox.Show("Файл Excel сохранен!", "Успех");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при экспорте в Excel");
                MessageBox.Show(Strings.Instance.ExportErrorMessage, Strings.Instance.ExportErrorTitle);
            }
        }
    }

    [RelayCommand]
    private async Task ExportXmlAsync()
    {
        var dialog = new SaveFileDialog { Filter = "XML Files (*.xml)|*.xml", DefaultExt = ".xml" };

        if (dialog.ShowDialog() == true)
        {
            try
            {

                var exporter = _exporters.FirstOrDefault(e => e.SupportedExtension == ".xml");

                if (exporter == null)
                {
                    System.Windows.MessageBox.Show("В системе не зарегистрирован модуль для экспорта в XML.", "Ошибка конфигурации");
                    return;
                }

                await exporter.ExportAsync(GetRecordsAsync(), dialog.FileName);
                System.Windows.MessageBox.Show("Файл XML сохранен!", "Успех");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при экспорте в XML");
                System.Windows.MessageBox.Show("Ошибка при экспорте. Подробности в логах.", "Ошибка");
            }
        }
    }
}
