
using ProcessingApp.Application;
using ProcessingApp.Infrastructure;
using System.Configuration;
using System.Data;
using System.Windows;
using Serilog;


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
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("Приложение запущено");

            try
            {
                using (var context = new AppDbContext())
                {
                    context.Database.EnsureCreated();
                }
            }
            catch (Exception ex)
            {
             
                Log.Fatal(ex, "Критическая ошибка при запуске приложения");
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
