using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcessingApp.Application.Interfaces;
using ProcessingApp.Infrastructure;
using ProcessingApp.Presentation.Localization;
using ProcessingApp.Presentation.Services;
using ProcessingApp.Presentation.State;
using ProcessingApp.Presentation.ViewModels;
using Serilog;
using System.Windows;

namespace ProcessingApp.Presentation;

public partial class App : System.Windows.Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        Configuration = BuildConfiguration();
        ConfigureLogging();

        Log.Information("Приложение запускается");

        _serviceProvider = BuildServiceProvider(Configuration);
    }

    public IConfiguration Configuration { get; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (!TryInitializeDatabase())
        {
            Shutdown();
            return;
        }

        _serviceProvider.GetRequiredService<MainWindow>().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("Приложение завершается");
        Log.CloseAndFlush();
        _serviceProvider.Dispose();

        base.OnExit(e);
    }

    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

    private static void ConfigureLogging() =>
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Debug()
            .WriteTo.File(
                "logs/app_log.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

    private static ServiceProvider BuildServiceProvider(IConfiguration configuration)
    {
        var services = new ServiceCollection();

        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        services.AddSingleton<IDialogService, WpfDialogService>();
        services.AddSingleton<SearchFilter>();

        services.AddTransient<IRecordRepository, RecordRepository>();
        services.AddTransient<ICsvImporter, CsvImporter>();
        services.AddTransient<IExporter, ExcelExporter>();
        services.AddTransient<IExporter, XmlExporter>();

        // ViewModel'и страниц — синглтоны: состояние (результаты поиска, выбранный файл)
        // сохраняется при переключении между разделами.
        services.AddSingleton<SearchViewModel>();
        services.AddSingleton<ImportViewModel>();
        services.AddSingleton<ExportViewModel>();
        services.AddSingleton<ShellViewModel>();

        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider();
    }

    private bool TryInitializeDatabase()
    {
        try
        {
            Log.Information("Проверка и инициализация базы данных...");

            var contextFactory = _serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var context = contextFactory.CreateDbContext();
            context.Database.EnsureCreated();

            return true;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Критическая ошибка при подключении к базе данных или её создании.");

            MessageBox.Show(
                Strings.Instance.StartupDbErrorMessage,
                Strings.Instance.StartupDbErrorTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            return false;
        }
    }
}
