using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using KinectFMT.Views;
using Microsoft.Kinect;
using Prism.Mvvm;

namespace KinectFMT.MVVModels
{
    public class MainSettingsModel : BindableBase
    {
        private bool _kinectEnable;

        public bool KinectEnable
        {
            get => _kinectEnable;
            set
            {
                _kinectEnable = value;
                RaisePropertyChanged($"KinectStatusColor");
                RaisePropertyChanged($"ProblemsVisibility");
                RaisePropertyChanged($"StartVisibility");
                RaisePropertyChanged($"KinectStatus");
            }
        }

        public ObservableCollection<string> Languages = new ObservableCollection<string>(){"English", "Русский"};
        public string CurrentLanguage
        {
            get =>
                App.Language.Name switch
                    {
                    "en-EN" => Languages[0],
                    "ru-RU" => Languages[1],
                    _ => "Language",
                    };
            set
            {
                if (value==Languages[0])
                    App.Language = new CultureInfo("en-En");
                if (value==Languages[1])
                    App.Language = new CultureInfo("ru-Ru");
            }
        }
        public void StartButton()
        {
            new MainWindow().Show();
        }
        public MainSettingsModel()
        {
            TestKinect();
        }
        public void TestKinect()
        {
            try
            {
                var kinectSensor = KinectSensor.GetDefault();
                kinectSensor.Open();
                var multiSource = kinectSensor.OpenMultiSourceFrameReader(
                    FrameSourceTypes.Color);
                Thread.Sleep(2500);
                KinectEnable = multiSource.KinectSensor.IsAvailable;
                kinectSensor.Close();
            }
            catch
            {
                KinectEnable = false;
            }
        }
    }
}
