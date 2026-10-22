using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProcessingApp.Application.Interfaces;
using ProcessingApp.Presentation.Localization;
using ProcessingApp.Presentation.Messages;
using Serilog;
using System.Globalization;

namespace ProcessingApp.Presentation.ViewModels;

public partial class ImportViewModel(
    ICsvImporter csvImporter,
    IDialogService dialogService,
    IMessenger messenger) : ObservableObject
{
    private static readonly CultureInfo CsvCulture = CultureInfo.GetCultureInfo("ru-RU");

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ImportCsvCommand))]
    private string? _selectedFilePath;

    private bool CanImport() => !string.IsNullOrWhiteSpace(SelectedFilePath);

    [RelayCommand]
    private void BrowseFile()
    {
        var filePath = dialogService.ShowOpenFileDialog("CSV Files (*.csv)|*.csv");

        if (filePath is not null)
        {
            SelectedFilePath = filePath;
        }
    }

    [RelayCommand(CanExecute = nameof(CanImport), IncludeCancelCommand = true)]
    private async Task ImportCsvAsync(CancellationToken token)
    {
        var strings = Strings.Instance;
        var filePath = SelectedFilePath!;

        try
        {
            await csvImporter.ImportCsvAsync(filePath, CsvCulture, token);

            dialogService.ShowMessage(strings.ImportSuccessMessage, strings.ImportSuccessTitle);
        }
        catch (OperationCanceledException)
        {
            Log.Information("Импорт CSV был отменен пользователем.");
            dialogService.ShowMessage(strings.ImportCancelledMessage, strings.ImportCancelledTitle);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при импорте файла: {FileName}", filePath);
            dialogService.ShowError(strings.ImportErrorMessage, strings.ImportErrorTitle);
        }
        finally
        {
            // Часть записей могла сохраниться и при отмене/ошибке (импорт идёт пачками),
            // поэтому таблицу на странице поиска обновляем в любом случае.
            messenger.Send(new RecordsImportedMessage());
        }
    }
}
