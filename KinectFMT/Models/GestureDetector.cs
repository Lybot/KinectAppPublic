using Microsoft.Kinect.VisualGestureBuilder;
using System.Collections.Generic;
using Microsoft.Kinect;
using MessageBox = System.Windows.MessageBox;

namespace KinectFMT.Models
{
    public class GestureDetector
    {
        public VisualGestureBuilderFrameSource Frame { get; set; }
        public VisualGestureBuilderFrameReader Reader { get; set; }
        public GestureDetector(VisualGestureBuilderFrameSource frame, VisualGestureBuilderFrameReader reader)
        {
            Frame = frame;
            Reader = reader;
        }
        public bool IsPaused
        {
            get => Reader.IsPaused;
            set => Reader.IsPaused = value;
        }

        public ulong TrackingId
        {
            get => Frame.TrackingId;
            set => Frame.TrackingId = value;
        }

        public void Dispose()
        {
            Frame?.Dispose();
            Reader?.Dispose();
        }
    }
}
