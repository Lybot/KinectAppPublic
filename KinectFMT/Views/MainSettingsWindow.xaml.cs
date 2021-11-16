using System.Windows;
using KinectFMT.MVVModels;
using KinectFMT.ViewModels;

namespace KinectFMT.Views
{
    /// <summary>
    /// Логика взаимодействия для MainSettingsWindow.xaml
    /// </summary>
    public partial class MainSettingsWindow : Window
    {
        public MainSettingsWindow()
        {
            InitializeComponent();
            MainSettingsVm vm = new MainSettingsVm();
            DataContext = vm;
            if (vm.CloseAction==null)
                vm.CloseAction = Close;
            LicenseModel.CloseWindow = Close;
        }
    }
}
