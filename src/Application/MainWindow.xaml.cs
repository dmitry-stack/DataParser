using Microsoft.Win32;
using ProcessingApp.Domain;
using ProcessingApp.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace ProcessingApp.Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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

                var importer = new CSVImport();
                await importer.ImportCsvAsync(dialog.FileName);

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

        private List<RecordDTO> GetFilteredData()
        {
            var repository = new RecordRepository();
            string? lastName = string.IsNullOrWhiteSpace(LastNameTextBox.Text) ? null : LastNameTextBox.Text;
            string? firstName = string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ? null : FirstNameTextBox.Text;
            string? surName = string.IsNullOrWhiteSpace(SurNameTextBox.Text) ? null : SurNameTextBox.Text;
            string? city = string.IsNullOrWhiteSpace(CityTextBox.Text) ? null : CityTextBox.Text;
            string? country = string.IsNullOrWhiteSpace(CountryTextBox.Text) ? null : CountryTextBox.Text;
            DateTime? date = MyDatePicker.SelectedDate;
            return repository.GetFilteredRecords(date, firstName, surName, country, city, lastName);
        }

        private void ExportExcelBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusLabel.Text = "Сбор данных для Excel...";


                var filteredData = GetFilteredData();
                if (filteredData.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    StatusLabel.Text = "Готов к работе";
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    DefaultExt = ".xlsx",
                    FileName = "exported_data"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    StatusLabel.Text = "Сохранение Excel файла...";

                    var exporter = new ExcelExporter();
                    exporter.ExportToExcel(filteredData, saveFileDialog.FileName);

                    StatusLabel.Text = "Экспорт в Excel успешно завершен!";
                    MessageBox.Show($"Данные успешно экспортированы в Excel! Всего записей: {filteredData.Count}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusLabel.Text = "Экспорт отменен";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusLabel.Text = "Ошибка";
            }
        }

        private void ExportXmlBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusLabel.Text = "Сбор данных для XML...";

               
                var filteredData = GetFilteredData();
                if (filteredData.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта по указанным фильтрам.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    StatusLabel.Text = "Готов к работе";
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "XML files (*.xml)|*.xml",
                    DefaultExt = ".xml",
                    FileName = "exported_data"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    StatusLabel.Text = "Сохранение XML файла...";

                    var exporter = new XmlExporter();
                    exporter.ExportToXml(filteredData, saveFileDialog.FileName);

                    StatusLabel.Text = "Экспорт в XML успешно завершен!";
                    MessageBox.Show($"Данные успешно экспортированы! Всего записей: {filteredData.Count}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusLabel.Text = "Экспорт отменен";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в XML: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusLabel.Text = "Ошибка";
            }
        }
        

        private void CheckFilterBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {

               
                var result = GetFilteredData();
                var displayList = result.Take(500).ToList();

                MyDataGrid.ItemsSource = displayList;

              StatusLabel.Text = $"Поиск завершен.\n Найдено всего: {result.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusLabel.Text = "Ошибка";
            }
        }
    }
    }
