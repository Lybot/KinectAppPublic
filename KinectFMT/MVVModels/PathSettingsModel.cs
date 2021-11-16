using System;
using System.IO;
using System.Windows.Forms;
using KinectFMT.Properties;
using Prism.Mvvm;

namespace KinectFMT.MVVModels
{
    class PathSettingsModel : BindableBase
    {
        public string BackgroundPath
        {
            get => Settings.Default.BackgroundPath;
            set
            {
                Settings.Default.BackgroundPath = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public string ForegroundPath
        {
            get => Settings.Default.ForegroundPath;
            set
            {
                Settings.Default.ForegroundPath = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public string BrandPath
        {
            get => Settings.Default.BrandPath;
            set
            {
                Settings.Default.BrandPath = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public string SavedImagesPath
        {
            get => Settings.Default.SavedImagesPath;
            set
            {
                Settings.Default.SavedImagesPath = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public string MasksPath
        {
            get => Settings.Default.MasksPath;
            set
            {
                Settings.Default.MasksPath = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public void BackgroundPathClick()
        {
            var dialog = new FolderBrowserDialog { RootFolder = Environment.SpecialFolder.MyComputer };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                BackgroundPath = dialog.SelectedPath;
                Settings.Default.Save();
            }
        }
        public void MasksPathClick()
        {
            var dialog = new FolderBrowserDialog { RootFolder = Environment.SpecialFolder.MyComputer };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                MasksPath = dialog.SelectedPath;
                Settings.Default.Save();
            }
        }
        public void BrandPathClick()
        {
            FileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                BrandPath = dialog.FileName;
                Settings.Default.Save();
            }
        }
        public void ForegroundPathClick()
        {
            FileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ForegroundPath = dialog.FileName;
                Settings.Default.Save();
            }
        }
        public void SavedImagesPathClick()
        {
            var dialog = new FolderBrowserDialog { RootFolder = Environment.SpecialFolder.MyComputer };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SavedImagesPath = dialog.SelectedPath;
                //OnPropertyChanged($"SavedImagesPath");
                var dir = new DirectoryInfo(dialog.SelectedPath + "\\BrandPhotos");
                if (!dir.Exists)
                    dir.Create();
                Settings.Default.Save();
            }
        }
    }
}
