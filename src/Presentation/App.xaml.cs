using Microsoft.Extensions.DependencyInjection;
using ProcessingApp.Application.Interfaces; 
using ProcessingApp.Infrastructure;

using ProcessingApp.Presentation.ViewModels;
using System.Windows;

namespace ProcessingApp.Presentation
{
    public partial class App : System.Windows.Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();

      
            services.AddTransient<IRecordRepository, RecordRepository>();
            services.AddTransient<ICsvImporter, CSVImport>();
            services.AddTransient<IExcelExporter, ExcelExporter>();
            services.AddTransient<IXmlExporter, XmlExporter>();

            services.AddTransient<MainViewModel>();

            services.AddTransient<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}