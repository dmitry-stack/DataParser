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
    }
}