using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using KinectFMT.MVVModels;
using KinectFMT.Properties;
using Prism.Commands;
using Prism.Mvvm;

namespace KinectFMT.ViewModels
{
    class PathSettingsVm : BindableBase
    {
        private PathSettingsModel _model = new PathSettingsModel();

        public string BackgroundPath
        {
            get => _model.BackgroundPath;
            set => _model.BackgroundPath = value;
        }
        public string ForegroundPath
        {
            get => _model.ForegroundPath;
            set => _model.ForegroundPath = value;
        }
        public string BrandPath
        {
            get => _model.BrandPath;
            set => _model.BrandPath = value;

        }
        public string SavedImagesPath
        {
            get => _model.SavedImagesPath;
            set => _model.SavedImagesPath = value;
        }
        public string MasksPath
        {
            get => _model.MasksPath;
            set => _model.MasksPath = value;
        }
        public DelegateCommand BackgroundPathClick { get; set; }
        public DelegateCommand ForegroundPathClick { get; set; }
        public DelegateCommand MasksPathClick { get; set; }
        public DelegateCommand BrandPathClick { get; set; }
        public DelegateCommand SavedImagesPathClick { get; set; }
        public PathSettingsVm()
        {
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
            BackgroundPathClick = new DelegateCommand(delegate
            {
                _model.BackgroundPathClick();
            });
            ForegroundPathClick = new DelegateCommand(delegate
            {
                _model.ForegroundPathClick();
            });
            MasksPathClick = new DelegateCommand(delegate
            {
                _model.MasksPathClick();
            });
            BrandPathClick = new DelegateCommand(delegate
            {
                _model.BrandPathClick();
            });
           SavedImagesPathClick = new DelegateCommand(delegate
            {
                _model.SavedImagesPathClick();
            });
        }
    }
}
