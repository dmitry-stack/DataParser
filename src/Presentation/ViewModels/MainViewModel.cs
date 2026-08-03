using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProcessingApp.Application;
using ProcessingApp.Application.Interfaces;
using Serilog;
using System.Collections.ObjectModel;
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
                System.Windows.MessageBox.Show("Не удалось выполнить поиск. Проверьте подключение к базе данных.", "Ошибка БД", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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

                    System.Windows.MessageBox.Show("Данные успешно импортированы!", "Успех", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                    await SearchAsync();
                }
                catch (OperationCanceledException)
                {
                  
                    Log.Information("Импорт CSV был отменен пользователем.");
                    MessageBox.Show("Импорт отменен.", "Отмена", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Ошибка при импорте файла: {FileName}", dialog.FileName);
                    System.Windows.MessageBox.Show("Произошла ошибка при импорте. Подробности в логах.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                //catch (Exception ex)
                //{
                //    Log.Error(ex, "Ошибка при импорте файла: {FileName}", dialog.FileName);


                //    string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;


                //    System.Windows.MessageBox.Show($"Произошла ошибка при сохранении в БД!\n\nПодробности:\n{errorMessage}",
                //                                   "Ошибка импорта",
                //                                   System.Windows.MessageBoxButton.OK,
                //                                   System.Windows.MessageBoxImage.Error);
                //}
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
                    System.Windows.MessageBox.Show("Файл Excel сохранен!", "Успех");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Ошибка при экспорте в Excel");
                    System.Windows.MessageBox.Show("Ошибка при экспорте. Подробности в логах.", "Ошибка");
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
}
