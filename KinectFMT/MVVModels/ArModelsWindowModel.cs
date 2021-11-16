using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Prism.Commands;
using Prism.Mvvm;

namespace KinectFMT.MVVModels
{
    public class ArModelsWindowModel:BindableBase
    {
        private static string _path = Environment.CurrentDirectory + "\\ArModels";
        private ObservableCollection<AddingArModel> _arModels = new ObservableCollection<AddingArModel>();
        private AddingArModel _selectedModel;
        public ObservableCollection<AddingArModel> ArModels
        {
            get => _arModels;
            set
            {
                _arModels = value;
                RaisePropertyChanged();
            }
        }

        public AddingArModel SelectedModel
        {
            get => _selectedModel;
            set
            {
                _selectedModel = value;
                RaisePropertyChanged();
            }
        }
        public void Closing()
        {
            var data = JsonConvert.SerializeObject(ArModels);
            File.WriteAllText(_path+"\\settings.json",data);
        }
        public void DeleteArModel(string source)
        {
            var arModel= ArModels.FirstOrDefault(it => it.Source == source);
            ArModels.Remove(arModel);
            if (arModel != null) File.Delete(arModel.Source);
        }
        public void AddArModel()
        {
            var dialog = new OpenFileDialog {Filter = @"Image(*.png, *.jpg)|*.*g"};
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var bitmap = new Bitmap(dialog.FileName);
                    bitmap.Dispose();
                    if (ArModels.Contains(ArModels.FirstOrDefault(it=>it.Source==_path+dialog.SafeFileName)))
                        return;
                    File.Copy(dialog.FileName, _path+"\\"+dialog.SafeFileName);
                    var arModel = new AddingArModel()
                        {Height = 100, Left = 0, Source = _path + "\\" + dialog.SafeFileName, Width = 100, Top = 0, Type = "Head", Delete = new DelegateCommand<string>(DeleteArModel)};
                    ArModels.Add(arModel);
                    SelectedModel = arModel;
                }
                catch 
                {
                    //ignored
                }
            }
        }
        public ArModelsWindowModel()
        {
            var dir = new DirectoryInfo(_path);
            if (!dir.Exists)
            {
                dir.Create();
                return;
            }
            var settingsPath = _path + "\\settings.json";
            if (!File.Exists(settingsPath))
                return;
            var jsonString = File.ReadAllText(settingsPath);
            ArModels = JsonConvert.DeserializeObject<ObservableCollection<AddingArModel>>(jsonString);
            foreach (var model in ArModels)
            {
                model.Delete = new DelegateCommand<string>(DeleteArModel);
            }
            SelectedModel = ArModels[0];
        }
    }

    public class AddingArModel:BindableBase
    {
        private string _type;
        private int _left;
        private int _top;
        private int _width;
        private int _height;
        private string _source;
        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                RaisePropertyChanged();
            }
        }
        public int Left
        {
            get => _left; 
            set
            {
                _left = value;
                RaisePropertyChanged();
            }
        }
        public int Top
        {
            get => _top;
            set
            {
                _top = value;
                RaisePropertyChanged();
            }
        }
        public int Width
        {
            get => _width;
            set
            {
                _width = value;
                RaisePropertyChanged();
            }
        }
        public int Height
        {
            get => _height;
            set
            {
                _height = value;
                RaisePropertyChanged();
            }
        }
        public string Source
        {
            get => _source;
            set
            {
                _source = value;
                RaisePropertyChanged();
            }
        }
        [JsonIgnore]
        public string VisualSource
        {
            get
            {
                var index = Source.LastIndexOf('\\');
                var lastIndex = Source.LastIndexOf('.');
                return Source.Substring(index+1, lastIndex - index-1);
            }
        }
        [JsonIgnore]
        public DelegateCommand<string> Delete { get; set; }
    }
}
