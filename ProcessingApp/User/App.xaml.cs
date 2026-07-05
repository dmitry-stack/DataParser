using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using ProcessingApp.Infrastructure;

namespace ProcessingApp.User
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
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

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
        }

