using System.Windows.Input;
using KinectFMT.MVVModels;
using KinectFMT.ViewModels;

namespace KinectFMT.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            MainWindowModel.CloseWindow = Close;
            InitializeComponent();
            var vm = new MainWindowVm();
            DataContext = vm;
        }
    }
}
