using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectFMT.MVVModels;
using Prism.Commands;
using Prism.Mvvm;

namespace KinectFMT.ViewModels
{
    class EmailSettingsVm : BindableBase
    {
        private EmailSettingsModel _model = new EmailSettingsModel();
        public string SmtpServer
        {
            get => _model.SmtpServer;
            set => _model.SmtpServer = value;
        }
        public string EmailPort
        {
            get => _model.EmailPort;
            set => _model.EmailPort = value;
        }
        public string RootEmail
        {
            get => _model.RootEmail;
            set => _model.RootEmail = value;
        }
        public string Representative
        {
            get => _model.Representative;
            set => _model.Representative = value;
        }
        public string EmailMessage
        {
            get => _model.EmailMessage;
            set => _model.EmailMessage = value;
        }
        public string EmailTitle
        {
            get => _model.EmailTitle;
            set => _model.EmailTitle = value;
        }
        public string PasswordEmail
        {
            get => _model.PasswordEmail;
            set => _model.PasswordEmail = value;
        }
        public DelegateCommand SendEmails { get; set; }
        public EmailSettingsVm()
        {
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
            SendEmails= new DelegateCommand(delegate
            {
                _model.SendEmails();
            });
        }
    }
}
