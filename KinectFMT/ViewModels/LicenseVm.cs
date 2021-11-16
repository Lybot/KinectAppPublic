using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KinectFMT.MVVModels;
using Prism.Commands;
using Prism.Mvvm;

namespace KinectFMT.ViewModels
{
    class LicenseVm:BindableBase
    {
        private LicenseModel _model = new LicenseModel();
        public DelegateCommand Deactivate { get; set; }
        public string ButtonText => _model.ButtonText;
        public Visibility ActivateVisibility => _model.ActivateVisibility;
        public Visibility DeactivateVisibility => _model.DeactivateVisibility;
        public LicenseVm()
        {
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
            Deactivate = new DelegateCommand(_model.Deactivate);
        }
    }
}
