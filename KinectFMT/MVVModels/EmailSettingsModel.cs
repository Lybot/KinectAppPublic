using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectFMT.Properties;
using KinectFMT.Views;
using Prism.Mvvm;

namespace KinectFMT.MVVModels
{
    class EmailSettingsModel : BindableBase
    {
        public string SmtpServer
        {
            get => Settings.Default.SmtpServer;
            set
            {
                Settings.Default.SmtpServer = value;
                Settings.Default.Save();
            }
        }
        public string EmailPort
        {
            get => Settings.Default.EmailPort.ToString();
            set
            {
                try
                {
                    Settings.Default.EmailPort = int.Parse(value);
                    Settings.Default.Save();
                }
                catch (Exception)
                {
                    //ignored
                }
            }
        }
        public string RootEmail
        {
            get => Settings.Default.RootEmail;
            set
            {
                Settings.Default.RootEmail = value;
                Settings.Default.Save();
            }
        }
        public string PasswordEmail
        {
            get => Settings.Default.PasswordEmail;
            set
            {
                Settings.Default.PasswordEmail = value;
                Settings.Default.Save();
            }
        }
        public string Representative
        {
            get => Settings.Default.Representative;
            set
            {
                Settings.Default.Representative = value;
                Settings.Default.Save();
            }
        }
        public string EmailMessage
        {
            get => Settings.Default.EmailMessage;
            set
            {
                Settings.Default.EmailMessage = value;
                Settings.Default.Save();
            }
        }
        public string EmailTitle
        {
            get => Settings.Default.EmailTitle;
            set
            {
                Settings.Default.EmailTitle = value;
                Settings.Default.Save();
            }
        }
        public void SendEmails()
        {
            SendEmails sendEmails = new SendEmails();
            sendEmails.Show();
        }
    }
}
