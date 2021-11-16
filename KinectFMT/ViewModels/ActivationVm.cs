using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using KinectFMT.MVVModels;
using Prism.Commands;
using Prism.Mvvm;

namespace KinectFMT.ViewModels
{
    public class ActivationVm : BindableBase
    {
        private ActivationModel _model = new ActivationModel();
        public Action CloseAction
        {
            get => _model.CloseWindow;
            set => _model.CloseWindow = value;
        }
        public DelegateCommand StartTrial { get; set; }
        public string Key
        {
            get => _model.Key;
            set => _model.Key = value;
        }
        public DelegateCommand Activate { get; set; }
        public Visibility ActivationVisibility
        {
            get => _model.ActivationVisibility;
            set => _model.ActivationVisibility = value;
        }
        public ActivationVm()
        {
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
            _model.Dispatcher= Dispatcher.CurrentDispatcher;
            StartTrial=new DelegateCommand(delegate
            {
                _model.StartTrial();
            });
            Activate = new DelegateCommand(delegate
            {
                _model.Activate();
            });
        }
    }
}
