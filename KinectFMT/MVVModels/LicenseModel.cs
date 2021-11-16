using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using KinectFMT.Models;
using KinectFMT.Views;
using Microsoft.Win32;
using Newtonsoft.Json;
using Prism.Mvvm;
using MessageBox = System.Windows.Forms.MessageBox;

namespace KinectFMT.MVVModels
{
    class LicenseModel:BindableBase
    {
        private const string Address = "http://yakovkholin1-001-site2.dtempurl.com/api/activate/deactivate";
        private readonly Dispatcher _dispatcher;
        public static Action CloseWindow { get; set; }

        public string ButtonText
        {
            get
            {
                if (Activation.IsTrial)
                    return Functions.FindStringResource("Activate");
                return Functions.FindStringResource("Deactivate");
            }
        }
        public Visibility ActivateVisibility
        {
            get
            {
                if (Activation.IsTrial)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }
        public Visibility DeactivateVisibility
        {
            get
            {
                if (!Activation.IsTrial)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }
        public void Deactivate()
        {
            if (!Activation.IsTrial)
                new Task(async delegate
                {
                    string baseKeyString = @"SOFTWARE\KinectFMT";
                    string key;
                    var registryKey = Registry.CurrentUser.OpenSubKey(baseKeyString, false);
                    if (registryKey == null)
                        return;
                    try
                    {
                        var data = registryKey.GetValue("ActivationData").ToString();
                        var decryptData = Functions.Decrypt(data, "4SlovaBol'ishimiBukvamiaaaaaaaaa");
                        string[] strings = decryptData.Split(' ');
                        key = strings[0];
                    }
                    catch (Exception)
                    {
                        Registry.CurrentUser.DeleteSubKey(baseKeyString);
                        new ActivationWindow().Show();
                        return;
                    }
                    var dataString = new StringBuilder();
                    ManagementObjectSearcher mysBios = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
                    foreach (var o in mysBios.Get())
                    {
                        var myBios = (ManagementObject)o;
                        dataString.Append(myBios["Name"]);
                    }

                    ManagementObjectSearcher mysProcessor = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                    foreach (var o in mysProcessor.Get())
                    {
                        var myProc = (ManagementObject)o;
                        dataString.Append(myProc["Name"]);
                    }

                    var hash = Functions.GetHashString(dataString.ToString());
                    var client = new HttpClient();
                    var request = new ActivationRequest()
                    {
                        Activate = false,
                        Key = key,
                        HashData = hash,
                    };
                    try
                    {
                        var jsonString = JsonConvert.SerializeObject(request);
                        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                        var response = await client.PostAsync(Address, content);
                        var responseString = response.Content.ReadAsStringAsync().Result;
                        var objectResponse = JsonConvert.DeserializeObject<Response>(responseString);
                        if (objectResponse.Result)
                        {
                            _dispatcher.Invoke(delegate
                            {
                                ActivationWindow main = new ActivationWindow();
                                Activation.IsTrial = true;
                                main.Show();
                            });
                            Registry.CurrentUser.DeleteSubKey(baseKeyString);
                        }
                        else
                        {
                            MessageBox.Show(Functions.FindStringResource("ServerError"), Functions.FindStringResource("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(Functions.FindStringResource("InternetProblems"), Functions.FindStringResource("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }).Start();
            else
            {
                new ActivationWindow().Show();
                CloseWindow();
            }
        }

        public LicenseModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }
    }
}
