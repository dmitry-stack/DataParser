using MahApps.Metro.Controls;
using ProcessingApp.Presentation.ViewModels;

namespace ProcessingApp.Presentation
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}