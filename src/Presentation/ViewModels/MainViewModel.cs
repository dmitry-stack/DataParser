using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProcessingApp.Application;
using ProcessingApp.Application.Interfaces;
using ProcessingApp.Presentation.Localization;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ProcessingApp.Presentation.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IRecordRepository _repository;
        private readonly IExcelExporter _excelExporter;
        private readonly IXmlExporter _xmlExporter;
        private readonly ICsvImporter _csvImporter;

        public MainViewModel(
            IRecordRepository repository,
            IExcelExporter excelExporter,
            IXmlExporter xmlExporter,
            ICsvImporter csvImporter)
        {
            _repository = repository;
            _excelExporter = excelExporter;
            _xmlExporter = xmlExporter;
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
                var result = _repository.GetFilteredRecordsAsync(Date, FirstName, SurName, City, Country, LastName);

                await foreach (var record in result)
                {
                    Records.Add(record);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при поиске записей в БД.");
                MessageBox.Show(
                    Strings.Instance.SearchErrorMessage,
                    Strings.Instance.SearchErrorTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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
                    await _csvImporter.ImportCsvAsync(dialog.FileName, token);

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
                    await _excelExporter.ExportToExcelAsync(GetRecordsAsync(), dialog.FileName);
                    MessageBox.Show(Strings.Instance.ExportExcelSuccessMessage, Strings.Instance.ExportSuccessTitle);
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
                    await _xmlExporter.ExportToXmlAsync(GetRecordsAsync(), dialog.FileName);
                    MessageBox.Show(Strings.Instance.ExportXmlSuccessMessage, Strings.Instance.ExportSuccessTitle);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Ошибка при экспорте в XML");
                    MessageBox.Show(Strings.Instance.ExportErrorMessage, Strings.Instance.ExportErrorTitle);
                }
            }
        }
    }
}