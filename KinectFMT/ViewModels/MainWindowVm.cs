using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using KinectFMT.Models;
using KinectFMT.MVVModels;
using Prism.Commands;
using Prism.Mvvm;

namespace KinectFMT.ViewModels
{
    public class MainWindowVm:BindableBase
    {
        private MainWindowModel _model = new MainWindowModel();
        public WriteableBitmap BodyBitmap => _model.BodyBitmap;
        public ImageSource BackgroundSource => _model.BackgroundSource;
        public CollectionArModels Canvas  => _model.Canvas;
        public ImageSource TableSource => _model.TableSource;
        public ImageSource NumberSource => _model.NumberSource;
        public ImageSource LeftSource => _model.LeftSource;
        public ImageSource RightSource => _model.RightSource;
        public ImageSource CenterSource => _model.CenterSource;
        public ImageSource Demo => _model.Demo;
        public Cursor Cursor => Cursors.None;
        public int WidthScreen
        {
            get => _model.WidthScreen;
            set => _model.WidthScreen = value;
        }
        public int HeightScreen
        {
            get => _model.HeightScreen;
            set => _model.HeightScreen = value;
        }

        public DelegateCommand Closing { get; set; }
        public DelegateCommand<object> KeyPressed { get; set; }
        public MainWindowVm()
        {
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
            Closing = new DelegateCommand(_model.ClosingWindow);
            KeyPressed = new DelegateCommand<object>(key=>{_model.KeyPressed(key);});
        }
    }
}
