using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectFMT.Models
{
    public class ActivationRequest
    {
        public string HashData { get; set; }
        public string Key { get; set; }
        public string Email { get; set; }
        public bool Activate { get; set; }
        public string AppName { get; set; } = "Kinect";
    }
}
