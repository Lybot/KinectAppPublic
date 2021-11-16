using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectFMT.MVVModels;
using Prism.Commands;
using Prism.Mvvm;

namespace KinectFMT.ViewModels
{
    public class ArModelsVm:BindableBase
    {
        private ArModelsWindowModel _model = new ArModelsWindowModel();

        public ObservableCollection<AddingArModel> ArModels
        {
            get => _model.ArModels;
            set => _model.ArModels = value;
        }
        public AddingArModel SelectedModel
        {
            get => _model.SelectedModel;
            set => _model.SelectedModel = value;
        }
        public DelegateCommand Closing { get; set; }
        public DelegateCommand AddArModel { get; set; }
        public DelegateCommand<string> DeleteArModel { get; set; }
        public ArModelsVm()
        {
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
            DeleteArModel = new DelegateCommand<string>((s)=>_model.DeleteArModel(s));
            AddArModel = new DelegateCommand(_model.AddArModel);
            Closing = new DelegateCommand(_model.Closing);
        }
    }
}
