using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectFMT.Models
{
    public class DetectorsCollection: ObservableCollection<GestureDetector>
    {
        public bool IsContainId(ulong trackingId)
        {
            foreach (var detector in this)
            {
                if (detector.TrackingId == trackingId)
                    return true;
            }
            return false;
        }

        public void StartDetector(ulong trackingId)
        {
            foreach (var detector in this)
            {
                if (detector.TrackingId == trackingId)
                {
                    detector.IsPaused = false;
                    return;
                }
            }
        }
        public void StopDetector(ulong trackingId)
        {
            foreach (var detector in this)
            {
                if (detector.TrackingId == trackingId)
                {
                    detector.IsPaused = true;
                    detector.TrackingId = 0;
                }

            }
        }
    }
}
