using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using KinectFMT.Properties;
using Microsoft.Kinect.Wpf.Controls;

namespace KinectFMT
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal KinectRegion KinectRegion { get; set; }
        public static event EventHandler LanguageChanged;

        public static CultureInfo Language
        {
            get => Thread.CurrentThread.CurrentUICulture;
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (value == Thread.CurrentThread.CurrentUICulture) return;
                //1. Меняем язык приложения:
                Thread.CurrentThread.CurrentUICulture = value;
                //2. Создаём ResourceDictionary для новой культуры
                ResourceDictionary dict = new ResourceDictionary();
                switch (value.Name)
                {
                    case "ru-RU":
                        dict.Source = new Uri($"Resources/Language.{value.Name}.xaml", UriKind.Relative);
                        break;
                    default:
                        dict.Source = new Uri("Resources/Language.xaml", UriKind.Relative);
                        break;
                }
                //3. Находим старую ResourceDictionary и удаляем его и добавляем новую ResourceDictionary
                ResourceDictionary oldDict = (from d in Current.Resources.MergedDictionaries
                                              where d.Source != null && d.Source.OriginalString.StartsWith("Resources/Language.")
                                              select d).First();
                if (oldDict != null)
                {
                    int ind = Current.Resources.MergedDictionaries.IndexOf(oldDict);
                    Current.Resources.MergedDictionaries.Remove(oldDict);
                    Current.Resources.MergedDictionaries.Insert(ind, dict);
                }
                else
                {
                    Current.Resources.MergedDictionaries.Add(dict);
                }
                //4. Вызываем евент для оповещения всех окон.
                LanguageChanged?.Invoke(Current, new EventArgs());
            }
        }
        public static List<CultureInfo> Languages { get; } = new List<CultureInfo>();
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Language = Settings.Default.DefaultLanguage;
        }
        private void App_LanguageChanged(object sender, EventArgs e)
        {
            Settings.Default.DefaultLanguage = Language;
            Settings.Default.Save();
        }
        public App()
        {
            LanguageChanged += App_LanguageChanged;
            Settings.Default.DPI = (int)Graphics.FromHwnd(IntPtr.Zero).DpiX;
            Settings.Default.Save();
        }
    }
}
