using Microsoft.Win32;
using ProcessingApp.Application;
using ProcessingApp.Application.Interfaces;
using ProcessingApp.Domain;
using System;
using System.Collections.Generic;
using System.Windows;

namespace ProcessingApp.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       
        private readonly IRecordRepository _repository;
        private readonly IExcelExporter _excelExporter;
        private readonly IXmlExporter _xmlExporter;
        private readonly ICsvImporter _csvImporter; 

        public MainWindow(
            IRecordRepository repository,
            IExcelExporter excelExporter,
            IXmlExporter xmlExporter,
            ICsvImporter csvImporter)
        {
            InitializeComponent();

            _repository = repository;
            _excelExporter = excelExporter;
            _xmlExporter = xmlExporter;
            _csvImporter = csvImporter;
        }

        private async void Choose_File_Btn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = ".csv",
                Filter = "CSV files (*.csv)|*.csv"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                Choose_File_Btn.IsEnabled = false;
                StatusLabel.Text = "Идет импорт данных, пожалуйста, подождите...";

                await _csvImporter.ImportCsvAsync(dialog.FileName);

                MessageBox.Show("Импорт успешно завершен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Choose_File_Btn.IsEnabled = true;
                StatusLabel.Text = "Готов к работе";
            }
        }

        private IAsyncEnumerable<RecordDTO> GetFilteredDataStream()
        {
            string? lastName = string.IsNullOrWhiteSpace(LastNameTextBox.Text) ? null : LastNameTextBox.Text;
            string? firstName = string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ? null : FirstNameTextBox.Text;
            string? surName = string.IsNullOrWhiteSpace(SurNameTextBox.Text) ? null : SurNameTextBox.Text;
            string? city = string.IsNullOrWhiteSpace(CityTextBox.Text) ? null : CityTextBox.Text;
            string? country = string.IsNullOrWhiteSpace(CountryTextBox.Text) ? null : CountryTextBox.Text;
            DateTime? date = MyDatePicker.SelectedDate;

            return _repository.GetFilteredRecordsAsync(date, firstName, surName, country, city, lastName);
        }

        private async void ExportExcelBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    DefaultExt = ".xlsx",
                    FileName = "exported_data"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    StatusLabel.Text = "Создание файла Excel...";
                    ExportExcelBtn.IsEnabled = false;

                    var stream = GetFilteredDataStream();

                    await _excelExporter.ExportToExcelAsync(stream, saveFileDialog.FileName);

                    StatusLabel.Text = "Экспорт в Excel успешно завершен!";
                    MessageBox.Show("Данные успешно экспортированы в Excel!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusLabel.Text = "Ошибка";
            }
            finally
            {
                ExportExcelBtn.IsEnabled = true;
            }
        }

        private async void ExportXmlBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "XML files (*.xml)|*.xml",
                    DefaultExt = ".xml",
                    FileName = "exported_data"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    StatusLabel.Text = "Сохранение XML файла...";
                    ExportXmlBtn.IsEnabled = false;

                    var stream = GetFilteredDataStream();

                    await _xmlExporter.ExportToXmlAsync(stream, saveFileDialog.FileName);

                    StatusLabel.Text = "Экспорт в XML успешно завершен!";
                    MessageBox.Show("Данные успешно экспортированы в XML!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в XML: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusLabel.Text = "Ошибка";
            }
            finally
            {
                ExportXmlBtn.IsEnabled = true;
            }
        }

        private async void CheckFilterBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusLabel.Text = "Поиск данных...";
                CheckFilterBtn.IsEnabled = false;

                var stream = GetFilteredDataStream();
                var displayList = new List<RecordDTO>();
                int totalCount = 0;

                await foreach (var record in stream)
                {
                    if (displayList.Count < 500)
                    {
                        displayList.Add(record);
                    }
                    totalCount++;
                }

                MyDataGrid.ItemsSource = displayList;
                StatusLabel.Text = $"Поиск завершен. \nНайдено всего: {totalCount} (Показано {displayList.Count})";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusLabel.Text = "Ошибка";
            }
            finally
            {
                CheckFilterBtn.IsEnabled = true;
            }
        }
    }
}