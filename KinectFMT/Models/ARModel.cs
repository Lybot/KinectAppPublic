using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using KinectFMT.MVVModels;
using KinectFMT.Properties;
using Microsoft.Kinect;
using Newtonsoft.Json;
using Prism.Mvvm;

namespace KinectFMT.Models
{
    public class ArModel:BindableBase
    {
        private int _top;
        private int _left;
        private int _width;
        private int _height;
        private string _source;
        private ulong _trackingId;
        public int SourceNumber=-1;
        private double GestureAccuracy => Settings.Default.GestureAccuracy;
        private double _leftSwipeGesture;
        private double _rightSwipeGesture;
        private double _leftClenchingGesture;
        private double _rightClenchingGesture;
        private double _clapGesture;
        private readonly bool _sourcesAvailable;
        public int Top
        {
            get => _top;
            set
            {
                _top = value;
                RaisePropertyChanged();
            }
        }
        public int Left
        {
            get => _left;
            set
            {
                _left = value;
                RaisePropertyChanged();
            }
        }
        public int Width
        {
            get => _width;
            set
            {
                _width = value;
                RaisePropertyChanged();
            }
        }
        public int Height
        {
            get => _height;
            set
            {
                _height = value;
                RaisePropertyChanged();
            }
        }
        public string Source
        {
            get => _source;
            set
            {
                _source = value;
                RaisePropertyChanged();
            }
        }
        public ulong TrackingId
        {
            get => _trackingId;
            set
            {
                _trackingId = value;
                RaisePropertyChanged();
            }
        }
        public Action LeftSwipeAction { get; set; }
        public Action RightSwipeAction { get; set; }
        public Action ClapAction { get; set; }
        public Action RightClenchingAction { get; set; }
        public Action LeftClenchingAction { get; set; }
        public double LeftSwipeGesture
        {
            get => _leftSwipeGesture;
            set
            {
                if (value > _leftSwipeGesture)
                {
                    if (value - _leftSwipeGesture < 0.4 - GestureAccuracy/5)
                    {
                        _leftSwipeGesture = value;
                        if (_leftSwipeGesture >= (0.7 + GestureAccuracy / 5) / 1.5)
                        {
                            _rightClenchingGesture = 0;
                            _leftClenchingGesture = 0;
                        }
                        if (_leftSwipeGesture >= 0.7 + GestureAccuracy/5)
                        {
                            LeftSwipeAction();
                            _leftSwipeGesture = 0;
                        }
                    }
                    else
                    {
                        _leftSwipeGesture = 0;
                    }
                }
                else
                {
                    _leftSwipeGesture = value;
                }
            }
        }
        public double RightSwipeGesture
        {
            get => _rightSwipeGesture;
            set
            {
                if (value > _rightSwipeGesture)
                {
                    if (value - _rightSwipeGesture < 0.4 - GestureAccuracy / 5)
                    {
                        if (_rightSwipeGesture >= (0.7 + GestureAccuracy / 5) / 1.5)
                        {
                            _rightClenchingGesture = 0;
                            _leftClenchingGesture = 0;
                        }
                        _rightSwipeGesture = value;
                        if (_rightSwipeGesture > 0.7 + GestureAccuracy / 5)
                        {
                            RightSwipeAction();
                            _rightSwipeGesture = 0;
                        }
                    }
                    else
                    {
                        _rightSwipeGesture = 0;
                    }
                }
                else
                {
                    _rightSwipeGesture = value;
                }
            }
        }
        public double ClapGesture
        {
            get => _clapGesture;
            set
            {
                if (value > _clapGesture)
                {
                    if (value - _clapGesture < 0.55 - GestureAccuracy / 5)
                    {
                        _clapGesture = value;
                        if (_clapGesture >= (0.7 + GestureAccuracy / 5) / 2)
                        {
                            _rightSwipeGesture = 0;
                            _leftSwipeGesture = 0;
                            _leftClenchingGesture = 0;
                            _rightClenchingGesture = 0;
                        }
                        if (_clapGesture > 0.6 + GestureAccuracy / 5)
                        {
                            ClapAction();
                            _clapGesture = 0;
                        }
                    }
                    else
                    {
                        _clapGesture = 0;
                    }
                }
                else
                {
                    _clapGesture = value;
                }
            }
        }
        public double LeftClenchingGesture
        {
            get => _leftClenchingGesture;
            set
            {
                if (value > _leftClenchingGesture)
                {
                    if (value - _leftClenchingGesture < 0.5 - GestureAccuracy / 5)
                    {
                        _leftClenchingGesture = value;
                        if (_leftClenchingGesture > 0.6 + GestureAccuracy / 5)
                        {
                            LeftClenchingAction();
                            _leftClenchingGesture = 0;
                        }
                    }
                    else
                    {
                        _leftClenchingGesture = 0;
                    }
                }
                else
                {
                    _leftClenchingGesture = value;
                }
            }
        }
        public double RightClenchingGesture
        {
            get => _rightClenchingGesture;
            set
            {
                if (value > _rightClenchingGesture)
                {
                    if (value - _rightClenchingGesture < 0.5 - GestureAccuracy / 5)
                    {
                        _rightClenchingGesture = value;
                        if (_rightClenchingGesture > 0.6 + GestureAccuracy / 5)
                        {
                            RightClenchingAction();
                            _rightClenchingGesture = 0;
                        }
                    }
                    else
                    {
                        _rightClenchingGesture = 0;
                    }
                }
                else
                {
                    _rightClenchingGesture = value;
                }
            }
        }
        public ColorSpacePoint Head { get; set; }
        public ColorSpacePoint LeftArm { get; set; }
        public ColorSpacePoint RightArm { get; set; }
        public Vector4 OrientationHead { get; set; }
        public Vector4 OrientationLeftArm { get; set; }
        public Vector4 OrientationRightArm { get; set; }
        public List<string> SourcesList { get; set; } = new List<string>();
        public List<Point> Offsets { get; set; } = new List<Point>();
        public List<string> Types { get; set; } = new List<string>();
        public List<Tuple<int,int>> Sizes { get; set; } = new List<Tuple<int, int>>();
        public ArModel(Action leftSwipeAction, Action rightSwipeAction, Action clapAction)
        {
            LeftSwipeAction = leftSwipeAction;
            RightSwipeAction = rightSwipeAction;
            ClapAction = clapAction;
        }
        public ArModel(int top, int left, ulong trackingId, Action leftSwipeAction, Action rightSwipeAction, Action clapAction)
        {
            LeftSwipeAction = leftSwipeAction;
            RightSwipeAction = rightSwipeAction;
            LeftClenchingAction = delegate{};
            RightClenchingAction = delegate {};
            ClapAction = clapAction;
            Source = null;
            Left = left;
            Top = top;
            TrackingId = trackingId;
            var settingsPath = Environment.CurrentDirectory + "\\ArModels\\settings.json";
            if (!File.Exists(settingsPath))
            {
                _sourcesAvailable = false;
                return;
            }
            var data =
                JsonConvert.DeserializeObject<ObservableCollection<AddingArModel>>(File.ReadAllText(settingsPath));
            if (data.Count == 0)
            {
                _sourcesAvailable = false;
                return;
            }
            foreach (var model in data)
            {
                SourcesList.Add(model.Source);
                Types.Add(model.Type);
                Offsets.Add(new Point(model.Left, model.Top));
                Sizes.Add(new Tuple<int, int>(model.Width,model.Height));
            }
            _sourcesAvailable = true;
            LeftClenchingAction = PreviousSource;
            RightClenchingAction = NextSource;
        }
        public ArModel(int top, int left, ulong trackingId, Action leftSwipeAction, Action rightSwipeAction, Action clapAction, Action leftClenchingAction, Action rightClenchingAction)
        {
            LeftSwipeAction = leftSwipeAction;
            RightSwipeAction = rightSwipeAction;
            LeftClenchingAction = delegate { };
            RightClenchingAction = delegate { };
            ClapAction = clapAction;
            Source = null;
            Left = left;
            Top = top;
            TrackingId = trackingId;
            var settingsPath = Environment.CurrentDirectory + "\\ArModels\\settings.json";
            if (!File.Exists(settingsPath))
            {
                _sourcesAvailable = false;
                return;
            }
            var data =
                JsonConvert.DeserializeObject<ObservableCollection<AddingArModel>>(File.ReadAllText(settingsPath));
            if (data.Count == 0)
            {
                _sourcesAvailable = false;
                return;
            }
            foreach (var model in data)
            {
                SourcesList.Add(model.Source);
                Types.Add(model.Type);
                Offsets.Add(new Point(model.Left, model.Top));
                Sizes.Add(new Tuple<int, int>(model.Width, model.Height));
            }
            _sourcesAvailable = true;
            LeftClenchingAction = leftClenchingAction;
            RightClenchingAction = rightClenchingAction;
        }
        public void UpdatePosition(ColorSpacePoint head, ColorSpacePoint rightArm, ColorSpacePoint leftArm, Vector4 orientationHead, Vector4 orientationLeftArm, Vector4 orientationRightArm)
        {
            Head = head;
            RightArm = rightArm;
            LeftArm = leftArm;
            OrientationHead = orientationHead;
            OrientationLeftArm = orientationLeftArm;
            OrientationRightArm = orientationRightArm;
            if (SourceNumber==-1||SourceNumber>=SourcesList.Count)
                return;
            switch (Types[SourceNumber])
            {
                case "Head":
                    Top = (int) Head.Y + Offsets[SourceNumber].Y;
                    Left = (int) Head.X + Offsets[SourceNumber].X;
                    return;
                case "RightHand":
                    Top = (int)RightArm.Y + Offsets[SourceNumber].Y;
                    Left = (int)RightArm.X + Offsets[SourceNumber].X;
                    return;
                case "LeftHand":
                    Top = (int)LeftArm.Y + Offsets[SourceNumber].Y;
                    Left = (int)LeftArm.X + Offsets[SourceNumber].X;
                    return;
            }
            //Graphics.FromImage(t)
            //Graphics gpra = Graphics.FromImage(Image.FromFile(Source));
            //gpra.RotateTransform();
        }
        public void NextSource()
        {
            if (!_sourcesAvailable)
                return;
            SourceNumber++;
            if (SourceNumber >= 0 && SourceNumber < SourcesList.Count)
            {
                Width = Sizes[SourceNumber].Item1;
                Height = Sizes[SourceNumber].Item2;
                Source = SourcesList[SourceNumber];
            }
            else
            {
                SourceNumber = -1;
                Source = null;
            }
        }
        public void PreviousSource()
        {
            if (!_sourcesAvailable)
                return;
            SourceNumber--;
            if (SourceNumber >= 0 && SourceNumber < SourcesList.Count)
            {
                Width = Sizes[SourceNumber].Item1;
                Height = Sizes[SourceNumber].Item2;
                Source = SourcesList[SourceNumber];
            }
            else
            {
                if (SourceNumber == -2)
                {
                    SourceNumber = SourcesList.Count - 1;
                    Width = Sizes[SourceNumber].Item1;
                    Height = Sizes[SourceNumber].Item2;
                    Source = SourcesList[SourceNumber];
                }
                SourceNumber = SourcesList.Count;
                Source = null;
            }
        }
    }
}
