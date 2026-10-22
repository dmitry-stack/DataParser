using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcessingApp.Application.Interfaces;
using ProcessingApp.Presentation.Localization;
using ProcessingApp.Presentation.State;
using Serilog;
using System.IO;
using Path = System.IO.Path;

namespace ProcessingApp.Presentation.ViewModels;

public partial class ExportViewModel : ObservableObject
{
    private const string ExcelExtension = ".xlsx";
    private const string XmlExtension = ".xml";

    private readonly IRecordRepository _repository;
    private readonly IEnumerable<IExporter> _exporters;
    private readonly IDialogService _dialogService;
    private readonly SearchFilter _filter;

    public ExportViewModel(
        IRecordRepository repository,
        IEnumerable<IExporter> exporters,
        IDialogService dialogService,
        SearchFilter filter)
    {
        _repository = repository;
        _exporters = exporters;
        _dialogService = dialogService;
        _filter = filter;

        _filter.PropertyChanged += (_, _) => OnPropertyChanged(nameof(FilterSummary));
        Strings.Instance.PropertyChanged += (_, _) => OnPropertyChanged(nameof(FilterSummary));
    }

    /// <summary>Человекочитаемое описание фильтров, по которым будет выполнена выгрузка.</summary>
    public string FilterSummary
    {
        get
        {
            var strings = Strings.Instance;
            var parts = new List<string>();

            if (_filter.Date is { } date)
            {
                parts.Add($"{strings.ColumnDate}: {date:dd.MM.yyyy}");
            }

            AddIfNotEmpty(parts, strings.ColumnLastName, _filter.LastName);
            AddIfNotEmpty(parts, strings.ColumnFirstName, _filter.FirstName);
            AddIfNotEmpty(parts, strings.ColumnSurName, _filter.SurName);
            AddIfNotEmpty(parts, strings.ColumnCity, _filter.City);
            AddIfNotEmpty(parts, strings.ColumnCountry, _filter.Country);

            return parts.Count == 0 ? strings.ExportFiltersNone : string.Join("; ", parts);
        }
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task ExportAsync(string extension, CancellationToken token)
    {
        var strings = Strings.Instance;
        var exporter = _exporters.FirstOrDefault(e => e.SupportedExtension == extension);

        if (exporter is null)
        {
            _dialogService.ShowError(strings.ExportMissingModuleErrorMessage, strings.ExportErrorTitle);
            return;
        }

        var filePath = _dialogService.ShowSaveFileDialog(GetFileFilter(extension), extension);

        if (filePath is null)
        {
            return;
        }

        // Пишем во временный файл рядом с целевым и подменяем его только при успехе:
        // отмена или ошибка не оставят недописанный файл и не затрут уже существующий.
        var tempPath = GetTempPath(filePath);

        try
        {
            var records = _repository.GetFilteredRecordsAsync(
                _filter.Date,
                _filter.FirstName,
                _filter.SurName,
                _filter.LastName,
                _filter.City,
                _filter.Country,
                pageNumber: 1,
                pageSize: int.MaxValue);

            await exporter.ExportAsync(records, tempPath, token);
            File.Move(tempPath, filePath, overwrite: true);

            var successMessage = extension == ExcelExtension
                ? strings.ExportExcelSuccessMessage
                : strings.ExportXmlSuccessMessage;

            _dialogService.ShowMessage(successMessage, strings.ExportSuccessTitle);
        }
        catch (OperationCanceledException)
        {
            Log.Information("Экспорт в {Extension} отменён пользователем.", extension);
            DeleteQuietly(tempPath);
            _dialogService.ShowMessage(strings.ExportCancelledMessage, strings.ExportCancelledTitle);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при экспорте в {Extension}", extension);
            DeleteQuietly(tempPath);
            _dialogService.ShowError(strings.ExportErrorMessage, strings.ExportErrorTitle);
        }
    }

    private static string GetTempPath(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        var name = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);

        return Path.Combine(directory, $"{name}.{Guid.NewGuid():N}.tmp{extension}");
    }

    private static string GetFileFilter(string extension) => extension switch
    {
        ExcelExtension => "Excel Files (*.xlsx)|*.xlsx",
        XmlExtension => "XML Files (*.xml)|*.xml",
        _ => "All Files (*.*)|*.*",
    };

    private static void AddIfNotEmpty(List<string> parts, string label, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parts.Add($"{label}: {value}");
        }
    }

    /// <summary>Убирает временный файл экспорта, оставшийся после отмены или ошибки.</summary>
    private static void DeleteQuietly(string tempPath)
    {
        try
        {
            File.Delete(tempPath);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Не удалось удалить временный файл экспорта {FilePath}", tempPath);
        }
    }
}
