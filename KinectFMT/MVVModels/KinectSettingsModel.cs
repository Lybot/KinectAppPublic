using System;
using System.Windows.Forms;
using KinectFMT.Properties;
using KinectFMT.Views;
using Microsoft.Kinect;
using Prism.Mvvm;

namespace KinectFMT.MVVModels
{
    public class KinectSettingsModel : BindableBase
    {
        public bool LowFps
        {
            get => Settings.Default.LowFps;
            set
            {
                Settings.Default.LowFps = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public bool WithTables
        {
            get => Settings.Default.WithTables;
            set
            {
                if (value)
                {
                    var rootFolder = Environment.CurrentDirectory;
                    if (!string.IsNullOrEmpty(Settings.Default.TablesPath))
                        rootFolder = Settings.Default.TablesPath;
                    var dialog = new FolderBrowserDialog() { SelectedPath = rootFolder };
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Settings.Default.TablesPath = dialog.SelectedPath;
                    }
                }
                Settings.Default.WithTables = value;
                Settings.Default.Save();
            }
        }
        public bool EnableEmails
        {
            get => Settings.Default.EnableEmails;
            set
            {
                Settings.Default.EnableEmails = value;
                Settings.Default.Save();
            }
        }
        public double MulCoefficient
        {
            get => Settings.Default.MulCoefficient;
            set
            {
                if (value < 0 || value > 3)
                    return;
                Settings.Default.MulCoefficient = Math.Round(value, 2);
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public float SigmaX
        {
            get => Settings.Default.SigmaX;
            set
            {
                if (value < 1 || value > 7)
                    return;
                Settings.Default.SigmaX = (float)Math.Round(value, 1);
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public double GammaCorrection
        {
            get => Settings.Default.GammaCorrection;
            set
            {
                var result = Math.Round(value, 1);
                Settings.Default.GammaCorrection = result;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public double GestureAccuracy
        {
            get => Settings.Default.GestureAccuracy;
            set
            {
                var result = Math.Round(value, 2);
                Settings.Default.GestureAccuracy = result;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public int ReloadMinutes
        {
            get => Settings.Default.ReloadMinutes;
            set
            {
                Settings.Default.ReloadMinutes = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public void SettingArModels()
        {
            var settings = new ArModelsWindow();
            settings.ShowDialog();
        }
        public KinectSettingsModel()
        {
        }
    }
}
