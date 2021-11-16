using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using KinectFMT.Models;
using KinectFMT.MVVModels;
using Prism.Commands;
using Prism.Mvvm;

namespace KinectFMT.ViewModels
{
    class MainSettingsVm : BindableBase
    {
        private MainSettingsModel _model = new MainSettingsModel();
        public ObservableCollection<string> Languages => _model.Languages;
        public string CurrentLanguage
        {
            get => _model.CurrentLanguage;
            set => _model.CurrentLanguage = value;
        }
        public DelegateCommand StartKinect { get; set; }
        public DelegateCommand PossibleProblems{ get; set; }
        public DelegateCommand TryAgain { get; set; }
        public Action CloseAction { get; set; }
        public string KinectStatus => _model.KinectEnable
            ? Functions.FindStringResource("KinectReady")
            : Functions.FindStringResource("KinectNotWorking");

        public Brush KinectStatusColor => _model.KinectEnable ? Brushes.DarkGreen : Brushes.DarkRed;
        public Visibility ProblemsVisibility => _model.KinectEnable ? Visibility.Collapsed : Visibility.Visible;
        public Visibility StartVisibility => _model.KinectEnable ? Visibility.Visible : Visibility.Collapsed;
        public MainSettingsVm()
        {
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
            StartKinect = new DelegateCommand(delegate
            {
                _model.StartButton();
                CloseAction();
            });
            TryAgain = new DelegateCommand(_model.TestKinect);
        }
    }
}
