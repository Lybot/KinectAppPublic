using KinectFMT.MVVModels;
using Prism.Commands;
using Prism.Mvvm;

namespace KinectFMT.ViewModels
{
    class KinectSettingsVm : BindableBase
    {
        private KinectSettingsModel _model = new KinectSettingsModel();
        public bool WithTables
        {
            get => _model.WithTables;
            set => _model.WithTables = value;
        }
        public bool EnableEmails
        {
            get => _model.EnableEmails;
            set => _model.EnableEmails = value;
        }
        public bool LowFps
        {
            get => _model.LowFps;
            set => _model.LowFps = value;
        }
        public double MulCoefficient
        {
            get => _model.MulCoefficient;
            set => _model.MulCoefficient = value;
        }
        //public double GammaCorrection
        //{
        //    get => _model.GammaCorrection;
        //    set => _model.GammaCorrection = value;
        //}
        public double GestureAccuracy
        {
            get => _model.GestureAccuracy*10;
            set => _model.GestureAccuracy = value/10;
        }
        public float SigmaX
        {
            get => _model.SigmaX;
            set => _model.SigmaX = value;
        }

        public int ReloadMinutes
        {
            get => _model.ReloadMinutes;
            set => _model.ReloadMinutes = value;
        }
        public DelegateCommand SettingsArModels { get; set; }
        public KinectSettingsVm()
        {
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
            SettingsArModels = new DelegateCommand(_model.SettingArModels);
        }
    }
}
