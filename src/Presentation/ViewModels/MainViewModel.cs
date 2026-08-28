using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProcessingApp.Application.DTOs;
using ProcessingApp.Application.Interfaces;
using ProcessingApp.Presentation.Localization;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace ProcessingApp.Presentation.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IRecordRepository _repository;

    private readonly ICsvImporter _csvImporter;

    private readonly IEnumerable<IExporter> _exporters;

    private readonly IDialogService _dialogService;
    private bool _hasNextPage;



    public MainViewModel(
        IRecordRepository repository,
        IEnumerable<IExporter> exporters,
        ICsvImporter csvImporter,
        IDialogService dialogService)
    {
        _repository = repository;
        _exporters = exporters;
        _csvImporter = csvImporter;
        _dialogService = dialogService;
        Strings.Instance.PropertyChanged += StringsOnPropertyChanged;
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

    public string CurrentPageDisplay => string.Format(Strings.Instance.PageFormat, CurrentPage);

    partial void OnCurrentPageChanged(int value)
    {
        OnPropertyChanged(nameof(CurrentPageDisplay));
    }

    [ObservableProperty]
    private int _pageSize = 15;

    [ObservableProperty]
    private ObservableCollection<RecordDTO> _records = new();



    private IAsyncEnumerable<RecordDTO> GetRecordsForExportAsync()
    {
        return _repository.GetFilteredRecordsAsync(
            Date, FirstName, SurName, LastName, City, Country,
            pageNumber: 1,
            pageSize: int.MaxValue);
    }

    [RelayCommand]
    private void SetLanguage(string cultureCode)
    {
        Strings.Instance.SetLanguage(cultureCode == "en" ? AppLanguage.English : AppLanguage.Russian);
    }

    private void StringsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(Strings.PageFormat))
        {
            OnPropertyChanged(nameof(CurrentPageDisplay));
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            Records.Clear();
            _hasNextPage = false; 

            var result = _repository.GetFilteredRecordsAsync(
                Date, FirstName, SurName, LastName, City, Country, CurrentPage, PageSize);

            int count = 0;
            await foreach (var record in result)
            {
                count++;

                if (count <= PageSize)
                {
                    Records.Add(record);
                }
                else
                {
                    _hasNextPage = true;
                    break;
                }
            }

            PreviousPageCommand.NotifyCanExecuteChanged();
            NextPageCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при поиске записей в БД.");
            _dialogService.ShowError("Не удалось выполнить поиск.", "Ошибка БД"); 
        }
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task ImportCsvAsync(CancellationToken token)
    {
        var filePath = _dialogService.ShowOpenFileDialog("CSV Files (*.csv)|*.csv");

        if (filePath != null)
        {
            try
            {
                var culture = new System.Globalization.CultureInfo("ru-RU");
                await _csvImporter.ImportCsvAsync(filePath, culture);

                _dialogService.ShowMessage(
                    Strings.Instance.ImportSuccessMessage,
                    Strings.Instance.ImportSuccessTitle);

                await SearchAsync();
            }
            catch (OperationCanceledException)
            {
                Log.Information("Импорт CSV был отменен пользователем.");
                _dialogService.ShowMessage(
                    Strings.Instance.ImportCancelledMessage,
                    Strings.Instance.ImportCancelledTitle);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при импорте файла: {FileName}", filePath);
                _dialogService.ShowError(
                    Strings.Instance.ImportErrorMessage,
                    Strings.Instance.ImportErrorTitle);
            }
        }
    }

    private bool CanGoPrevious() => CurrentPage > 1;

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private async Task PreviousPageAsync()
    {
        CurrentPage--;
        await SearchAsync();
    }

    private bool CanGoNext() => _hasNextPage;

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextPageAsync()
    {
        CurrentPage++;
        await SearchAsync();
    }


    [RelayCommand]
    private async Task ExecuteNewSearchAsync()
    {

        CurrentPage = 1;
        await SearchAsync();
    }
    [RelayCommand]
    private async Task ExportExcelAsync()
    {
     
        var filePath = _dialogService.ShowSaveFileDialog("Excel Files (*.xlsx)|*.xlsx", ".xlsx");

        if (filePath != null)
        {
            try
            {
                var exporter = _exporters.FirstOrDefault(e => e.SupportedExtension == ".xlsx");
                if (exporter == null)
                {
                    _dialogService.ShowError("Модуль экспорта не найден.", "Ошибка");
                    return;
                }

                await exporter.ExportAsync(GetRecordsForExportAsync(), filePath);
                _dialogService.ShowMessage("Файл Excel успешно сохранен!", "Успех");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка экспорта");
                _dialogService.ShowError("Ошибка при экспорте. Подробности в логах.", "Ошибка");
            }
        }
    }

    [RelayCommand]
    private async Task ExportXmlAsync()
    {
        var filePath = _dialogService.ShowSaveFileDialog("XML Files (*.xml)|*.xml", ".xml");

        if (filePath != null)
        {
            try
            {
                var exporter = _exporters.FirstOrDefault(e => e.SupportedExtension == ".xml");

                if (exporter == null)
                {
                    _dialogService.ShowError("В системе не зарегистрирован модуль для экспорта в XML.", "Ошибка конфигурации");
                    return;
                }

                await exporter.ExportAsync(GetRecordsForExportAsync(), filePath);
                _dialogService.ShowMessage("Файл XML сохранен!", "Успех");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при экспорте в XML");
                _dialogService.ShowError("Ошибка при экспорте. Подробности в логах.", "Ошибка");
            }
        }
    }
}
