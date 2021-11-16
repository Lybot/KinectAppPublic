using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using KinectFMT.MVVModels;
using Prism.Commands;
using Prism.Mvvm;

namespace KinectFMT.ViewModels
{
    public class CheckActivationVm:BindableBase
    {
        private CheckActivationModel _model = new CheckActivationModel();
        public Action CloseAction
        {
            get => _model.CloseWindow;
            set => _model.CloseWindow = value;
        }
        public CheckActivationVm()
        {
            _model.Dispatcher=Dispatcher.CurrentDispatcher;
            Test= new DelegateCommand(delegate
            {
                _model.CheckActivation();
            });
        }

        public DelegateCommand Test { get; set; }
    }
}
