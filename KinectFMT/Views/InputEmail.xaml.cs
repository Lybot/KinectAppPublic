using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using KinectFMT.Models;
using KinectFMT.Properties;

namespace KinectFMT.Views
{
    /// <summary>
    /// Логика взаимодействия для InputEmail.xaml
    /// </summary>
    public partial class InputEmail : Window
    {
        public string AttachmentPath;
        public InputEmail(string attachmentPath)
        {
            AttachmentPath = attachmentPath;
            InitializeComponent();
            InputBox.Focus();
            PreviewKeyDown += KeyLogger;
        }

        private void EnterClick(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(InputBox.Text)|| !InputBox.Text.Contains("@"))
                return;
            Processing.SendEmail(Settings.Default.SmtpServer, Settings.Default.RootEmail,
                Settings.Default.PasswordEmail, Settings.Default.EmailPort, InputBox.Text,
                Settings.Default.EmailMessage, Settings.Default.EmailTitle, AttachmentPath);
            Close();
        }
        private void KeyLogger(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }
    }
}
