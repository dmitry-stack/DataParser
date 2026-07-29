using Microsoft.Extensions.DependencyInjection;
using ProcessingApp.Application.Interfaces; 
using ProcessingApp.Infrastructure;

using Microsoft.Extensions.Configuration; 
using Microsoft.EntityFrameworkCore; 


using ProcessingApp.Presentation.ViewModels;
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

            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

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

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                dbContext.Database.EnsureCreated();
            }

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}