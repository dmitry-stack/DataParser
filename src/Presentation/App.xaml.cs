
using ProcessingApp.Application;
using ProcessingApp.Infrastructure;
using System.Configuration;
using System.Data;
using System.Windows;


namespace ProcessingApp.Presentation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);


            try
            {
                using (var context = new AppDbContext())
                {
                    context.Database.EnsureCreated();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Критическая ошибка:\n{ex.Message}\n",
                                "Ошибка инициализации",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);

                Current.Shutdown();
                return; 
            }
            var repository = new RecordRepository();
            var excelExporter = new ExcelExporter();
            var xmlExporter = new XmlExporter();
            var csvImporter = new CSVImport();

            var mainWindow = new MainWindow(repository, excelExporter, xmlExporter, csvImporter);
            mainWindow.Show();
        }
    }
        }
