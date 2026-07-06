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

        private void ExportExcelBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExportXmlBtn_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Search_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                var repository = new RecordRepository();

                string? cityFilter = CityTextBox.Text;
                string? lastNameFilter = LastNameTextBox.Text;
                string? firstNameFilter = FirstNameTextBox.Text;
                string? surNameFilter = SurNameTextBox.Text;
                string? countryFilter = CountryTextBox.Text;

                DateTime? dateFilter = MyDatePicker.SelectedDate;

                List<RecordDTO> filteredData = repository.GetFilteredRecords(
                    dateFilter,
                    firstNameFilter,
                    surNameFilter,
                    countryFilter,
                    cityFilter,
                    lastNameFilter
                );


                MessageBox.Show($"Найдено записей\nв базе: {filteredData.Count}", "Результат фильтрации");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске: {ex.Message}");
            }
        }

        private void CheckFilterBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                var repository = new RecordRepository();

                string? lastName = string.IsNullOrWhiteSpace(LastNameTextBox.Text) ? null : LastNameTextBox.Text;
                string? firstName = string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ? null : FirstNameTextBox.Text;
                string? surName = string.IsNullOrWhiteSpace(SurNameTextBox.Text) ? null : SurNameTextBox.Text;
                string? city = string.IsNullOrWhiteSpace(CityTextBox.Text) ? null : CityTextBox.Text;
                string? country = string.IsNullOrWhiteSpace(CountryTextBox.Text) ? null : CountryTextBox.Text;
                DateTime? date = MyDatePicker.SelectedDate;

                var result = repository.GetFilteredRecords(date, firstName, surName, country, city, lastName);

                var displayList = result.Take(500).ToList();

                MyDataGrid.ItemsSource = displayList;

              StatusLabel.Text = $"Поиск завершен. Найдено всего: {result.Count} (Отображено первые {displayList.Count})";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusLabel.Text = "Ошибка";
            }
        }
    }
    }
