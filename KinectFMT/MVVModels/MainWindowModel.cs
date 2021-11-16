using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using KinectFMT.Models;
using KinectFMT.Properties;
using KinectFMT.Views;
using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using Prism.Mvvm;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using FontFamily = System.Drawing.FontFamily;
using Pen = System.Drawing.Pen;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Size = System.Drawing.Size;

namespace KinectFMT.MVVModels
{
    public class MainWindowModel : BindableBase
    {
        private readonly int _bytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
        private readonly MultiSourceFrameReader _multiSource;
        private readonly BodyFrameReader _bodyFrameReader;
        private readonly CoordinateMapper _coordinateMapper;
        private List<string> ImageSources { get; set; } = new List<string>();
        private List<string> TableSources { get; set; } = new List<string>();
        private int _reloadMinutes
        {
            get
            {
                if (Settings.Default.ReloadMinutes == 0)
                    return int.MaxValue;
                return Settings.Default.ReloadMinutes;
            }
        }
        private readonly DepthSpacePoint[] _colorMappedToDepthPoints;
        private readonly DetectorsCollection _detectors = new DetectorsCollection();
        private readonly List<Gesture> _gestures = new List<Gesture>();
        private readonly WMPLib.WindowsMediaPlayer _pl = new WMPLib.WindowsMediaPlayer { URL = @"Common\Click.mp3" };
        private ImageSource _leftSource;
        private ImageSource _rightSource;
        private ImageSource _centerSource;
        private ImageSource _backgroundSource;
        private ImageSource _tableSource;
        private ImageSource _numberSource;
        private ImageSource _demo;
        private KinectSensor KinectSensor { get; set; }
        private Dispatcher Disp { get; set; }
        private readonly string _path = @"Common\NewKinect.gbd";
        private static bool WithTables => Settings.Default.WithTables;
        private static bool EnableEmails => Settings.Default.EnableEmails;
        private Thread _reloadThread;
        private bool _wasReload = true;
        private float SigmaX => Settings.Default.SigmaX;
        private int NumberBackground { get; set; } = -1;
        private int NumberTable { get; set; }
        private readonly uint _bitmapBackBufferSize;
        private bool LowFps => Settings.Default.LowFps;
        private bool _withBackground;
        private bool _backgroundSelected;
        private bool _isPreview;
        private bool _wasPrinted;
        private bool _wasSent;
        private bool _gesturePaused;
        private Body[] _bodies;
        private string _previewFilePath;
        private bool GestureIsPaused
        {
            get => _gesturePaused;
            set
            {
                if (value)
                {
                    foreach (var detector in _detectors)
                    {
                        detector.IsPaused = true;
                    }
                }
                else
                {
                    foreach (var detector in _detectors)
                    {
                        if (detector.TrackingId != 0)
                            detector.IsPaused = false;
                    }
                }

                _gesturePaused = value;
            }
        }

        public int WidthScreen { get; set; } = 1920;
        public int HeightScreen { get; set; } = 1080;
        public bool BackgroundSelected
        {
            get => _backgroundSelected;
            set
            {
                _backgroundSelected = value;
                if (value)
                {
                    Canvas.UpdateActions(PreviousTable, NextTable, ScreenShotStart);
                    Disp.Invoke(delegate { CenterSource = Functions.BitmapToImageSource(Resources.photo, false); });
                }
            }
        }
        public bool IsPreview
        {
            get => _isPreview;
            set
            {
                _isPreview = value;
                if (value)
                {
                    if (EnableEmails)
                        Canvas.UpdateActions(Repeat, Repeat, PrintPhoto, SendEmail, SendEmail);
                    else
                        Canvas.UpdateActions(Repeat, Repeat, PrintPhoto, delegate { }, delegate { });
                }
                else
                {
                    if (WithTables)
                        Canvas.UpdateActions(PreviousBackGround, NextBackGround, delegate { BackgroundSelected = true; });
                    else
                        Canvas.UpdateActions(PreviousBackGround, NextBackGround, ScreenShotStart);
                }
            }
        }
        public ImageSource LeftSource
        {
            get => _leftSource;
            set
            {
                _leftSource = value;
                RaisePropertyChanged();
            }
        }
        public ImageSource RightSource
        {
            get => _rightSource;
            set
            {
                _rightSource = value;
                RaisePropertyChanged();
            }
        }
        public ImageSource CenterSource
        {
            get => _centerSource;
            set
            {
                _centerSource = value;
                RaisePropertyChanged();
            }
        }

        public ImageSource Demo
        {
            get => _demo;
            set
            {
                _demo = value;
                RaisePropertyChanged();
            }
        }
        public ImageSource BackgroundSource
        {
            get => _backgroundSource;
            set
            {
                _backgroundSource = value;
                RaisePropertyChanged();
            }
        }
        public ImageSource TableSource
        {
            get => _tableSource;
            set
            {
                _tableSource = value;
                RaisePropertyChanged();
            }
        }
        public ImageSource NumberSource
        {
            get => _numberSource;
            set
            {
                _numberSource = value;
                RaisePropertyChanged();
            }
        }
        public WriteableBitmap BodyBitmap { get; set; }
        public CollectionArModels Canvas { get; set; } = new CollectionArModels();
        // ReSharper disable once CollectionNeverQueried.Local

        public MainWindowModel()
        {
            Disp = Dispatcher.CurrentDispatcher;
            string backPath = Settings.Default.BackgroundPath;
            using (var db = new VisualGestureBuilderDatabase(_path))
            {
                foreach (var gesture in db.AvailableGestures)
                {
                    _gestures.Add(gesture);
                }
            }
            DirectoryInfo dir = new DirectoryInfo(backPath);
            if (!dir.Exists)
            {
                dir.Create();
            }
            foreach (var file in dir.GetFiles())
            {
                ImageSources.Add(file.FullName);
            }
            KinectSensor = KinectSensor.GetDefault();
            KinectSensor.Open();
            var emailTxt = File.Open(Settings.Default.SavedImagesPath + "\\Email.txt", FileMode.OpenOrCreate);
            emailTxt.Flush();
            emailTxt.Dispose();
            _multiSource =
                KinectSensor.OpenMultiSourceFrameReader(
                    FrameSourceTypes.Depth | FrameSourceTypes.Color | FrameSourceTypes.BodyIndex);
            _multiSource.MultiSourceFrameArrived += MultiSourceFrameArrived;
            _multiSource.IsPaused = true;
            _bodyFrameReader = KinectSensor.BodyFrameSource.OpenReader();
            _bodyFrameReader.FrameArrived += Reader_BodyFrameArrived;
            _coordinateMapper = KinectSensor.CoordinateMapper;
            FrameDescription colorFrameDescription = KinectSensor.ColorFrameSource.FrameDescription;
            int colorWidth = colorFrameDescription.Width;
            int colorHeight = colorFrameDescription.Height;
            if (_reloadMinutes != int.MaxValue)
            {
                _reloadThread = new Thread(() =>
                {
                    Thread.Sleep(new TimeSpan(0, _reloadMinutes, 0));
                    _wasReload = false;
                });
                _reloadThread.Start();
            }

            //CvInvoke.UseOpenCL = false;
            _colorMappedToDepthPoints = new DepthSpacePoint[colorWidth * colorHeight];
            KinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            BodyBitmap = new WriteableBitmap(colorWidth, colorHeight, Settings.Default.DPI, Settings.Default.DPI, PixelFormats.Bgra32, null);
            _bitmapBackBufferSize = (uint)((BodyBitmap.BackBufferStride * (BodyBitmap.PixelHeight - 1)) + (BodyBitmap.PixelWidth * _bytesPerPixel));
            CvInvoke.NumThreads = 6;
            RightSource = Functions.BitmapToImageSource(Resources.nextarrow, false);
            LeftSource = Functions.BitmapToImageSource(Resources.backarrow, false);
            CenterSource = Functions.BitmapToImageSource(Resources.photo, false);
            for (int i = 0; i < 6; i++)
            {
                var frame = new VisualGestureBuilderFrameSource(KinectSensor, 0);
                var reader = frame.OpenReader();
                reader.IsPaused = true;
                frame.TrackingIdLost += TrackingIdLost;
                frame.AddGestures(_gestures);
                reader.FrameArrived += GestureFrameArrived;
                _detectors.Add(new GestureDetector(frame, reader));
            }
            if (WithTables)
            {
                CenterSource = Functions.BitmapToImageSource(Resources.Pick, false);
                //ScreenShotButton.Content = FindResource("Pick");
                DirectoryInfo dirTables = new DirectoryInfo(Settings.Default.TablesPath);
                foreach (var table in dirTables.GetFiles())
                {
                    TableSources.Add(table.FullName);
                }
            }
            if (Activation.IsTrial)
            {
                var bitmap = new Bitmap(WidthScreen - 14, HeightScreen - 14);
                var gra = Graphics.FromImage(bitmap);
                gra.DrawRectangle(new Pen(Color.Transparent), 0, 0, WidthScreen - 14, HeightScreen - 14);
                gra.DrawString("Demo", new Font(FontFamily.GenericSerif, 60), Brushes.Brown, 10, 10);
                gra.DrawString("Demo", new Font(FontFamily.GenericSerif, 60), Brushes.Brown, WidthScreen - 300, 10);
                gra.DrawString("Demo", new Font(FontFamily.GenericSerif, 60), Brushes.Brown, 10, HeightScreen - 100);
                gra.DrawString("Demo", new Font(FontFamily.GenericSerif, 60), Brushes.Brown, WidthScreen - 300, HeightScreen - 100);
                gra.Save();
                gra.Dispose();
                Demo = Functions.BitmapToImageSource(bitmap, true);
            }
        }
        private DateTime LastFrame { get; set; }
        private void MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            if (LowFps)
            {
                var time = DateTime.Now;
                if (Math.Abs(time.Millisecond - LastFrame.Millisecond) < 50)
                    return;
                LastFrame = time;
            }
            DepthFrame depthFrame = null;
            ColorFrame colorFrame = null;
            BodyIndexFrame bodyIndexFrame = null;
            bool isBitmapLocked = false;
            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();
            // If the Frame has expired by the time we process this event, return.
            if (multiSourceFrame == null)
            {
                return;
            }
            try
            {
                depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame();
                colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame();
                bodyIndexFrame = multiSourceFrame.BodyIndexFrameReference.AcquireFrame();
                if ((depthFrame == null) || (colorFrame == null) || (bodyIndexFrame == null))
                {
                    try
                    {
                        if (IsPreview)
                        {
                            _multiSource.IsPaused = true;
                            _multiSource.IsPaused = false;
                        }
                    }
                    catch (Exception)
                    {
                        //ignored
                    }
                    return;

                }
                FrameDescription depthFrameDescription = depthFrame.FrameDescription;
                var depthWidth = depthFrameDescription.Width;
                var depthHeight = depthFrameDescription.Height;
                using (KinectBuffer depthFrameData = depthFrame.LockImageBuffer())
                {
                    var origin = new Mat(new Size(depthWidth, depthHeight), DepthType.Cv16U, 1, depthFrameData.UnderlyingBuffer, 0);
                    var fixedDepth = new Mat();
                    CvInvoke.MedianBlur(origin, fixedDepth, 5);
                    _coordinateMapper.MapColorFrameToDepthSpaceUsingIntPtr(
                        fixedDepth.DataPointer,
                         depthFrameData.Size,
                         _colorMappedToDepthPoints);
                }
                depthFrame.Dispose();
                depthFrame = null;
                BodyBitmap.Lock();
                isBitmapLocked = true;
                colorFrame.CopyConvertedFrameDataToIntPtr(BodyBitmap.BackBuffer, _bitmapBackBufferSize, ColorImageFormat.Bgra);
                colorFrame.Dispose();
                colorFrame = null;
                if (!_withBackground)
                {
                    BodyBitmap.AddDirtyRect(new Int32Rect(0, 0, BodyBitmap.PixelWidth, BodyBitmap.PixelHeight));
                }
                else
                {
                    using KinectBuffer bodyIndexData = bodyIndexFrame.LockImageBuffer();
                    unsafe
                    {
                        byte* bodyIndexDataPointer = (byte*)bodyIndexData.UnderlyingBuffer;
                        int colorMappedToDepthPointCount = _colorMappedToDepthPoints.Length;
                        fixed (DepthSpacePoint* colorMappedToDepthPointsPointer = _colorMappedToDepthPoints)
                        {
                            byte* bitmapPixelsPointer = (byte*)BodyBitmap.BackBuffer;
                            for (int colorIndex = 0; colorIndex < colorMappedToDepthPointCount - 1; colorIndex += 3)
                            {
                                float colorMappedToDepthX = colorMappedToDepthPointsPointer[colorIndex].X;
                                float colorMappedToDepthY = colorMappedToDepthPointsPointer[colorIndex].Y;
                                if (!float.IsNegativeInfinity(colorMappedToDepthX) &&
                                    !float.IsNegativeInfinity(colorMappedToDepthY))
                                {
                                    // Make sure the depth pixel maps to a valid point in color space
                                    int depthX = (int)(colorMappedToDepthX + 0.5f);
                                    int depthY = (int)(colorMappedToDepthY + 0.5f);
                                    // If the point is not valid, there is no body index there.
                                    if ((depthX >= 0) && (depthX < depthWidth) && (depthY >= 0) &&
                                        (depthY < depthHeight))
                                    {
                                        int depthIndex = (depthY * depthWidth) + depthX;
                                        // ReSharper disable once PossibleNullReferenceException
                                        if (bodyIndexDataPointer[depthIndex] != 0xff)
                                        {
                                            continue;
                                        }
                                    }
                                }
                                // ReSharper disable once PossibleNullReferenceException
                                bitmapPixelsPointer[colorIndex * 4 + 3] = 0;
                                bitmapPixelsPointer[(colorIndex + 1) * 4 + 3] = 0;
                                bitmapPixelsPointer[(colorIndex + 2) * 4 + 3] = 0;
                            }
                        }
                    }
                    var mat = new Mat(new Size(1920, 1080), DepthType.Cv8U, 4, BodyBitmap.BackBuffer, 0);
                    VectorOfMat vectorOfMat = new VectorOfMat(mat.Split());
                    CvInvoke.GaussianBlur(vectorOfMat[3], vectorOfMat[3], new Size(0, 0), SigmaX);
                    CvInvoke.Merge(vectorOfMat, mat);
                    vectorOfMat.Dispose();
                    BodyBitmap.AddDirtyRect(new Int32Rect(0, 0, BodyBitmap.PixelWidth, BodyBitmap.PixelHeight));
                }
            }
            finally
            {
                if (isBitmapLocked)
                {
                    Disp.Invoke(BodyBitmap.Unlock);
                }
                bodyIndexFrame?.Dispose();
                depthFrame?.Dispose();
                colorFrame?.Dispose();
                //multiEnable = true;
            }
        }
        public void TrackingIdLost(object sender, TrackingIdLostEventArgs e)
        {
            _detectors.StopDetector(e.TrackingId);
            Canvas.DeleteArModel(e.TrackingId);
        }
        private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using BodyFrame bodyFrame = e.FrameReference.AcquireFrame();
            if (bodyFrame != null)
            {
                if (_bodies == null)
                {
                    _bodies = new Body[bodyFrame.BodyCount];
                }
                bodyFrame.GetAndRefreshBodyData(_bodies);
                int count = 0;
                int numberBody = 0;
                foreach (var body in _bodies)
                {
                    if (body.TrackingId == 0)
                    {
                        count++;
                        continue;
                    }
                    var positionHead = body.Joints[JointType.Head];
                    var positionLeftArm = body.Joints[JointType.HandTipLeft];
                    var positionRightArm = body.Joints[JointType.HandTipRight];
                    var orientationHead = body.JointOrientations[JointType.Head];
                    var orientationLeftArm = body.JointOrientations[JointType.HandTipLeft];
                    var orientationRightArm = body.JointOrientations[JointType.HandTipRight];
                    var colorPositionHead = _coordinateMapper.MapCameraPointToColorSpace(positionHead.Position);
                    var colorPositionLeftArm = _coordinateMapper.MapCameraPointToColorSpace(positionLeftArm.Position);
                    var colorPositionRightArm = _coordinateMapper.MapCameraPointToColorSpace(positionRightArm.Position);
                    if (!_detectors.IsContainId(body.TrackingId))
                    {
                        if (_detectors[numberBody].TrackingId != 0)
                            Canvas.DeleteArModel(_detectors[numberBody].TrackingId);
                        _detectors[numberBody].TrackingId = body.TrackingId;
                        _detectors[numberBody].IsPaused = false;
                        ArModel arModel = null;
                        if (!IsPreview)
                        {

                            if (WithTables)
                            {
                                if (BackgroundSelected)
                                {
                                    arModel = new ArModel((int)colorPositionHead.Y, (int)colorPositionHead.X,
                                        body.TrackingId, PreviousTable, NextTable,
                                        ScreenShotStart);
                                }
                                else
                                {
                                    arModel = new ArModel((int)colorPositionHead.Y, (int)colorPositionHead.X,
                                        body.TrackingId, PreviousBackGround, NextBackGround, delegate { BackgroundSelected = true; });
                                }
                            }
                            else
                                arModel = new ArModel((int)colorPositionHead.Y, (int)colorPositionHead.X,
                                    body.TrackingId, PreviousBackGround, NextBackGround, ScreenShotStart);
                        }
                        else
                        {
                            if (EnableEmails)
                            {
                                arModel = new ArModel((int)colorPositionHead.Y, (int)colorPositionHead.X,
                                    body.TrackingId, Repeat, Repeat, PrintPhoto, SendEmail, SendEmail);
                            }
                            else
                            {
                                arModel = new ArModel((int)colorPositionHead.Y, (int)colorPositionHead.X,
                                    body.TrackingId, Repeat, Repeat, PrintPhoto, delegate{}, delegate{});
                            }
                        }
                        Canvas.Add(arModel);
                        RaisePropertyChanged($"Canvas");
                        RaisePropertyChanged($"ItemsControl");
                    }
                    else
                    {
                        if (!GestureIsPaused)
                            _detectors.StartDetector(body.TrackingId);
                        Canvas.UpdateArModel(body.TrackingId, colorPositionHead, colorPositionRightArm, colorPositionLeftArm, orientationHead.Orientation, orientationLeftArm.Orientation, orientationRightArm.Orientation);
                        RaisePropertyChanged($"Canvas");
                        RaisePropertyChanged($"ItemsControl");
                    }
                    numberBody++;
                }
                if (count == 6)
                {
                    _multiSource.IsPaused = true;
                    if (!_wasReload)
                    {
                        KinectSensor.Close();
                        KinectSensor.Open();
                        _wasReload = true;
                        _reloadThread = new Thread(() =>
                        {
                            Thread.Sleep(new TimeSpan(0, _reloadMinutes, 0));
                            _wasReload = false;
                        });
                        _reloadThread.Start();
                    }
                }
                else
                {
                    if (!IsPreview)
                    {
                        _multiSource.IsPaused = false;
                        if (!_wasReload)
                        {
                            KinectSensor.Close();
                            KinectSensor.Open();
                            _wasReload = true;
                            _reloadThread = new Thread(() =>
                            {
                                Thread.Sleep(new TimeSpan(0, _reloadMinutes, 0));
                                _wasReload = false;
                            });
                            _reloadThread.Start();
                        }
                    }
                }
            }
        }
        private void GestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            var frameReference = e.FrameReference;
            using VisualGestureBuilderFrame frame = frameReference.AcquireFrame();
            IReadOnlyDictionary<Gesture, ContinuousGestureResult> continuousResults = frame?.ContinuousGestureResults;
            if (continuousResults != null)
            {
                foreach (var gesture in continuousResults.Keys)
                {
                    if (gesture.Name.Equals("Clap"))
                    {
                        continuousResults.TryGetValue(gesture, out var result);
                        if (result != null)
                        {
                            Canvas.UpdateArModel(frame.TrackingId, "clap", result.Progress);
                        }
                    }
                    if (gesture.Name.Equals("RightSwipe"))
                    {
                        continuousResults.TryGetValue(gesture, out var result);
                        if (result != null)
                        {
                            Canvas.UpdateArModel(frame.TrackingId, "rightSwipe", result.Progress);
                        }
                    }
                    if (gesture.Name.Equals("LeftSwipe"))
                    {
                        continuousResults.TryGetValue(gesture, out var result);
                        if (result != null)
                        {
                            Canvas.UpdateArModel(frame.TrackingId, "leftSwipe", result.Progress);
                        }
                    }
                    if (gesture.Name.Equals("LeftClenching"))
                    {
                        continuousResults.TryGetValue(gesture, out var result);
                        if (result != null)
                        {
                            Canvas.UpdateArModel(frame.TrackingId, "leftClenching", result.Progress);
                        }
                    }
                    if (gesture.Name.Equals("RightClenching"))
                    {
                        continuousResults.TryGetValue(gesture, out var result);
                        if (result != null)
                        {
                            Canvas.UpdateArModel(frame.TrackingId, "rightClenching", result.Progress);
                        }
                    }
                }
            }
        }
        public void PreviousBackGround()
        {
            NumberBackground--;
            if (NumberBackground >= 0 && NumberBackground < ImageSources.Count)
            {
                _withBackground = true;
                BackgroundSource = new BitmapImage(new Uri(ImageSources[NumberBackground], UriKind.Absolute));
            }
            else
            {
                if (NumberBackground == -2)
                {
                    NumberBackground = ImageSources.Count - 1;
                    BackgroundSource = new BitmapImage(new Uri(ImageSources[NumberBackground], UriKind.Absolute));
                    return;
                }
                NumberBackground = ImageSources.Count;
                _withBackground = false;
                BackgroundSource = null;
            }
        }
        public void NextBackGround()
        {
            NumberBackground++;
            if (NumberBackground >= 0 && NumberBackground < ImageSources.Count)
            {
                _withBackground = true;
                BackgroundSource = new BitmapImage(new Uri(ImageSources[NumberBackground], UriKind.Absolute));
            }
            else
            {
                NumberBackground = -1;
                _withBackground = false;
                BackgroundSource = null;
            }
        }
        public void PreviousTable()
        {
            NumberTable--;
            if (NumberTable >= 0 && NumberTable < TableSources.Count)
            {
                TableSource = new BitmapImage(new Uri(TableSources[NumberTable], UriKind.Absolute));
            }
            else
            {
                if (NumberTable == -2)
                {
                    NumberTable = TableSources.Count - 1;
                    TableSource = new BitmapImage(new Uri(TableSources[NumberTable], UriKind.Absolute));
                    return;
                }
                NumberTable = TableSources.Count;
                TableSource = null;
            }
        }
        public void NextTable()
        {
            NumberTable++;
            if (NumberTable >= 0 && NumberTable < TableSources.Count)
            {
                TableSource = new BitmapImage(new Uri(TableSources[NumberTable], UriKind.Absolute));
            }
            else
            {
                NumberTable = -1;
                TableSource = null;
            }
        }
        public void ScreenShotStart()
        {
            GestureIsPaused = true;
            //DoubleAnimation buttonAnimation = new DoubleAnimation
            //{
            //    From = 200,
            //    To = 300,
            //    FillBehavior = FillBehavior.Stop,
            //    Duration = TimeSpan.FromSeconds(1.5)
            //};
            //TimeImage.BeginAnimation(HeightProperty, buttonAnimation);
            //TimeImage.BeginAnimation(WidthProperty, buttonAnimation);
            NumberSource = Functions.BitmapToImageSource(Resources.need3, false);
            new Thread(() =>
            {
                Thread.Sleep(1500);
                Disp.Invoke(delegate
                {
                    try
                    {
                        NumberSource = Functions.BitmapToImageSource(Resources.need2, false);
                        //TimeImage.BeginAnimation(HeightProperty, buttonAnimation);
                        //TimeImage.BeginAnimation(WidthProperty, buttonAnimation);
                    }
                    catch (Exception)
                    {
                        //ignored
                    }
                });
                Thread.Sleep(1500);
                Disp.Invoke(delegate
                {
                    try
                    {
                        NumberSource = Functions.BitmapToImageSource(Resources._1, false);
                        //TimeImage.BeginAnimation(HeightProperty, buttonAnimation);
                        //TimeImage.BeginAnimation(WidthProperty, buttonAnimation);
                    }
                    catch (Exception)
                    {
                        //ignored
                    }
                });
                Thread.Sleep(1500);
                _pl.controls.play();
                try
                {
                    ScreenShot();
                }
                catch (Exception)
                {
                    //ignored
                }
            }).Start();
        }
        private void ScreenShot()
        {

            var clearOperation = Disp.InvokeAsync(delegate
           {
               IsPreview = true;
               _multiSource.IsPaused = true;
               NumberSource = null;
               LeftSource = null;
               RightSource = null;
               CenterSource = null;
           }, DispatcherPriority.Send);
            clearOperation.Wait();
            Thread.Sleep(100);
            var getPhoto = Disp.InvokeAsync(delegate
            {
                BodyBitmap.TryLock(new Duration(new TimeSpan(0, 0, 5)));
                var resultImage = new Image<Bgra, byte>(1920, 1080, 0, BodyBitmap.BackBuffer);
                var coefficient = Settings.Default.MulCoefficient;
                //var gammaCorrection = Settings.Default.GammaCorrection;
                resultImage[0] = resultImage[0].Mul(coefficient);
                resultImage[1] = resultImage[1].Mul(coefficient);
                resultImage[2] = resultImage[2].Mul(coefficient);
                resultImage._EqualizeHist();
                BodyBitmap.AddDirtyRect(new Int32Rect(0, 0, BodyBitmap.PixelWidth, BodyBitmap.PixelHeight));
                BodyBitmap.Unlock();
            }, DispatcherPriority.Send);
            getPhoto.Wait();
            Thread.Sleep(100);
            Bitmap screenshot = new Bitmap((int)SystemParameters.FullPrimaryScreenWidth, (int)SystemParameters.FullPrimaryScreenHeight, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(screenshot);
            g.CopyFromScreen(0, 0, 0, 0, new Size((int)SystemParameters.FullPrimaryScreenWidth, (int)SystemParameters.FullPrimaryScreenHeight), CopyPixelOperation.SourceCopy);
            g.Save();
            g.Dispose();
            try
            {
                string time = DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);
                string myPhotos = Settings.Default.SavedImagesPath;
                string path = Path.Combine(myPhotos, "Photo " + time + ".png");
                _previewFilePath = path;
                _tempBitmap = screenshot.Clone() as Bitmap;
                screenshot.Dispose();
                Disp.Invoke(Preview);
            }
            catch (IOException)
            {
                //ignored
            }
        }

        private Bitmap _tempBitmap;
        private void Preview()
        {
            RightSource = Functions.BitmapToImageSource(Resources.Repeat, false);
            LeftSource = Functions.BitmapToImageSource(Resources.Repeat, false);
            CenterSource = Functions.BitmapToImageSource(Resources.Print, false);
            Canvas.ClearArModels();
            BodyBitmap.Lock();
            var photo = Processing.ProcessPhoto(_tempBitmap, Settings.Default.BrandPath,
                Settings.Default.SavedImagesPath + "\\" + _previewFilePath.Substring(_previewFilePath.Length - 18, 18));
            Marshal.Copy(photo, 0, BodyBitmap.BackBuffer, (int)_bitmapBackBufferSize);
            BodyBitmap.AddDirtyRect(new Int32Rect(0, 0, BodyBitmap.PixelWidth, BodyBitmap.PixelHeight));
            BodyBitmap.Unlock();
            //BodyBitmap.Lock();
            //NextButton.Content = FindResource("Print");
            //PreviousButton.Content = FindResource("Email");
            //ScreenShotButton.Content = FindResource("Repeat");
            GestureIsPaused = false;
        }
        private void PrintPhoto()
        {
            if (!_wasPrinted)
            {
                Processing.Print(Settings.Default.SavedImagesPath + "\\" + _previewFilePath.Substring(_previewFilePath.Length - 18, 18));
                _wasPrinted = true;
            }
            Repeat();
        }
        private void SendEmail()
        {
            if (_wasSent)
                return;
            _wasSent = true;
            var inputEmail = new InputEmail(Settings.Default.SavedImagesPath + "\\" + _previewFilePath.Substring(_previewFilePath.Length - 18, 18));
            inputEmail.ShowDialog();
            inputEmail.Focus();
        }
        public void Repeat()
        {
            GC.Collect();
            //BodyBitmap.Unlock();
            _multiSource.IsPaused = false;
            IsPreview = false;
            _wasPrinted = false;
            _wasSent = false;
            TableSource = null;
            NumberTable = -1;
            Disp.Invoke(delegate
            {
                RightSource = Functions.BitmapToImageSource(Resources.nextarrow, false);
                LeftSource = Functions.BitmapToImageSource(Resources.backarrow, false);
                CenterSource = Functions.BitmapToImageSource(WithTables ? Resources.Pick : Resources.photo, false);
            });
        }
        public void ClosingWindow()
        {
            _multiSource?.Dispose();
            _bodyFrameReader?.Dispose();
            foreach (var detector in _detectors)
            {
                detector?.Dispose();
            }
            KinectSensor.Close();
            if (_reloadMinutes != int.MaxValue)
                _reloadThread.Abort();
        }

        public void KeyPressed(object args)
        {
            if (args as string == "F5")
            {
                var clear = Disp.InvokeAsync(ClosingWindow);
                clear.Wait();
                var window = new MainSettingsWindow();
                window.Show();
                CloseWindow();
            }
        }
        public static Action CloseWindow { get; set; }
    }
}
