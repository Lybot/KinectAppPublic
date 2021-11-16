using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using KinectFMT.Properties;

namespace KinectFMT.Views
{
    /// <summary>
    /// Логика взаимодействия для SendEmails.xaml
    /// </summary>
    public partial class SendEmails : Window, INotifyPropertyChanged
    {
        private Thread _thread;
        public List<DataEmail> Emails = new List<DataEmail>();
        private Dispatcher _disp;
        private SmtpClient _client = new SmtpClient(Settings.Default.SmtpServer, Settings.Default.EmailPort);
        private Array _data;
        public SendEmails()
        {
            InitializeComponent();
            DataContext = this;
            _disp = Dispatcher;
            StreamReader reader = new StreamReader(Settings.Default.SavedImagesPath + "\\Email.txt");
            while (!reader.EndOfStream)
            {
                string email = reader.ReadLine();
                var split = email?.Split(' ');
                bool result = split?[3] == "true";
                var dataEmail = new DataEmail(split?[0],split?[1]+" "+split?[2],result);
                Emails.Add(dataEmail);
            }
            reader.Close();
            reader.Dispose();
            _client.Credentials = new NetworkCredential(Settings.Default.RootEmail, Settings.Default.PasswordEmail);
            _client.EnableSsl = true;
            OnPropertyChanged($"Emails");
            DataGrid.ItemsSource = Emails;
            _thread = new Thread(() =>
            {
                foreach (var cell in _data)
                {
                    var info = cell as DataEmail;
                    if (info == null)
                        continue;
                    if (info.Sent)
                        continue;
                    var rootEmail = new MailAddress(Settings.Default.RootEmail);
                    var recipient = new MailAddress(info.Email);
                    var message = new MailMessage(rootEmail, recipient);
                    message.Attachments.Add(
                        new Attachment(Settings.Default.SavedImagesPath + "\\BrandPhotos\\" + info.Photo));
                    message.Body = Settings.Default.EmailMessage;
                    message.Subject = Settings.Default.EmailTitle;
                    _client.Send(message);
                    Emails.Remove(info);
                    info.Sent = true;
                    Emails.Add(info);
                    OnPropertyChanged($"Emails");
                }
                //_disp.Invoke(() =>
                //{
                //    DataGrid.ItemsSource = null;
                //    DataGrid.ItemsSource = Emails;
                //});
            });
        }
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void SendEmail(object sender, RoutedEventArgs e)
        {
            if(_thread.IsAlive)
                return;
            _data = new object[Emails.Count];
            DataGrid.SelectedItems.CopyTo(_data,0);
            _thread = new Thread(() =>
            {
                foreach (var cell in _data)
                {
                    var info = cell as DataEmail;
                    if (info == null)
                        return;
                    if (info.Sent)
                        return;
                    var rootEmail = new MailAddress(Settings.Default.RootEmail);
                    var recipient = new MailAddress(info.Email);
                    var message = new MailMessage(rootEmail, recipient);
                    message.Attachments.Add(
                        new Attachment(Settings.Default.SavedImagesPath + "\\BrandPhotos\\" + info.Photo));
                    message.Body = Settings.Default.EmailMessage;
                    message.Subject = Settings.Default.EmailTitle;
                    _client.Send(message);
                    Emails.Remove(info);
                    info.Sent = true;
                    Emails.Add(info);
                    OnPropertyChanged($"Emails");
                }
                //_disp.Invoke(() =>
                //{
                //    DataGrid.ItemsSource = null;
                //    DataGrid.ItemsSource = Emails;
                //});
            });
            _thread.Start();
        }

        private void ClearTable(object sender, RoutedEventArgs e)
        {
            var path = Settings.Default.SavedImagesPath + "\\Email.txt";
            File.Delete(path);
            _ = File.Create(path);
        }
    }

    public class DataEmail:INotifyPropertyChanged
    {
        private string _email;
        private string _photo;
        private bool _sent;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }
        public string Photo
        {
            get => _photo;
            set
            {
                _photo = value;
                OnPropertyChanged();
            }
        }
        public bool Sent
        {
            get => _sent;
            set
            {
                _sent = value;
                OnPropertyChanged();
            }
        }

        public DataEmail(string email, string photo, bool sent)
        {
            Email = email;
            Photo = photo;
            Sent = sent;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
