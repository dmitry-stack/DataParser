using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcessingApp.Application.Interfaces;
using ProcessingApp.Infrastructure;
using ProcessingApp.Presentation.ViewModels;
using Serilog;
using System.Windows;

namespace ProcessingApp.Presentation
{
    public partial class App : System.Windows.Application
    {
        private ServiceProvider _serviceProvider;
        public IConfiguration Configuration { get; private set; }
        public App()
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            Configuration = builder.Build();


            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Debug()
                .WriteTo.File("logs/app_log.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
            Log.Information("Приложение запускается");

            var services = new ServiceCollection();

            services.AddDbContextFactory<AppDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));


            services.AddTransient<IRecordRepository, RecordRepository>();
            services.AddTransient<ICsvImporter, CSVImport>();
            services.AddTransient<IExporter, ExcelExporter>();
            services.AddTransient<IExporter, XmlExporter>();

            services.AddTransient<MainViewModel>();
            services.AddTransient<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();
        }
        protected override void OnExit(ExitEventArgs e)
        {

            Log.Information("Приложение завершается");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                Log.Information("Проверка и инициализация базы данных...");
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    dbContext.Database.EnsureCreated();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Критическая ошибка при подключении к базе данных или её создании.");
                MessageBox.Show("Не удалось подключиться к базе данных. Проверьте настройки SQL Server.\n\nПодробности записаны в файл логов.",
                                "Критическая ошибка запуска",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);

                Current.Shutdown();
                return;
            }

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}