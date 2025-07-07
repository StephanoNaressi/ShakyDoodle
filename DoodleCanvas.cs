using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShakyDoodle
{
    public class DoodleCanvas : Control
    {
        #region Fields

        List<Stroke> _strokes;
        Stroke _currentStroke;

        ColorType _currentColor;
        SizeType _currentSize;
        int _gridSize;
        double _alpha;

        Random _r = new();
        double _time;

        float _amp = 2;
        float _offset = 7;
        float _speed = 0.3f;
        int _maxStrokes;

        Pen _mainPen;
        PenLineCap _currentCap;
        Dictionary<Point, Vector> _shakeSeeds = new();

        bool _isShake = true;
        bool _isInvalidating = false;
        Pen _gridPen = new(Brushes.LightBlue, 1);
        private bool _needsRedraw = false;

        #endregion

        #region Systems

        public DoodleCanvas()
        {
            _strokes = new List<Stroke>(_maxStrokes);

            Focusable = true;
            Focus();
            PointerPressed += (s, e) => Focus();

            PointerMoved += OnPointerMoved;
            PointerPressed += OnPointerPressed;
            PointerReleased += OnPointerReleased;

            KeyDown += OnKeyDown;

            _gridSize = 50;
            _alpha = 1;
            _currentCap = PenLineCap.Square;
            _maxStrokes = 300;
            Cursor = new Cursor(StandardCursorType.Cross);

            //_ = new PointerPointProperties();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            StartRenderLoopAsync();
        }

        private async void RequestInvalidate()
        {
            if (!_isShake)
                _needsRedraw = true;
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            var isCtrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            var isAlt = e.KeyModifiers.HasFlag(KeyModifiers.Alt);

            if (isCtrl && e.Key == Key.Z)
            {
                if (_strokes == null || _strokes.Count <= 0) return;
                _strokes.Remove(_strokes.Last());
                RequestInvalidate();
            }
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs events) => _currentStroke = null;


        private void OnPointerPressed(object? sender, PointerPressedEventArgs events)
        {
            if (events.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _currentStroke = new(_currentColor, events.GetPosition(this), _currentSize, _alpha, _currentCap, events.GetCurrentPoint(this).Properties.Pressure, _isShake);
                _strokes.Add(_currentStroke);
                System.Diagnostics.Debug.WriteLine($"Strokes: {_strokes.Count}");
                RequestInvalidate();
            }
        }

        private void OnPointerMoved(object? sender, PointerEventArgs events)
        {
            if (_currentStroke == null || !events.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

            var pos = events.GetPosition(this);

            if (_currentStroke.Points.Count == 0 || Distance(pos, _currentStroke.Points.Last()) > 5)
            {
                _currentStroke.Points.Add(pos);
                _currentStroke.Pressures.Add(events.GetCurrentPoint(this).Properties.Pressure);
                RequestInvalidate();
            }
        }


        private async void StartRenderLoopAsync()
        {
            while (true)
            {
                if (_isShake)
                {
                    _time += _speed;
                    InvalidateVisual();
                }
                else if (_needsRedraw)
                {
                    InvalidateVisual();
                    _needsRedraw = false;
                }

                await Task.Delay(16);
            }
        }


        #endregion

        #region Utility

        private Point GetShakenPoint(Point point, double shakeIntensity)
        {
            if (!_isShake || shakeIntensity <= 0) return point;

            double seedX = Math.Sin(point.X * 12.9898 + point.Y * 78.233);
            double seedY = Math.Cos(point.X * 93.9898 + point.Y * 67.345);

            double offsetX = (seedX * 43758.5453) % 2 - 1;
            double offsetY = (seedY * 12737.2349) % 2 - 1;

            double shakenX = point.X + Math.Sin(_time + offsetX * 10) * _amp * shakeIntensity;
            double shakenY = point.Y + Math.Cos(_time + offsetY * 10) * _amp * shakeIntensity;

            return new Point(shakenX, shakenY);
        }


        private double Distance(Point p1, Point p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private double GetShakeIntensity(int strokeIndex)
        {
            int count = _strokes.Count;
            int newer = count - 1 - strokeIndex;
            double t = newer / (double)_maxStrokes;
            return Math.Clamp(1.0 - t, 0.0, 1.0);
        }

        #endregion

        #region Rendering

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            
            //PushPop to avoid drawing outside the canvas
            using var clip = context.PushClip(new Rect(Bounds.Size));

            context.FillRectangle(Brushes.White, new Rect(Bounds.Size));

            DrawGrid(context);

            // Draw all existing strokes with shake effect based on their index
            for (int i = 0; i < _strokes.Count; i++)
            {
                double shakeIntensity = GetShakeIntensity(i);
                DrawStroke(_strokes[i], shakeIntensity, context);
            }
        }

        private void DrawGrid(DrawingContext context)
        {
            for (int i = 0; i <= Bounds.Width; i += _gridSize)
            {
                context.DrawLine(_gridPen, new Point(i, 0), new Point(i, Bounds.Height));
            }
            for (int j = 0; j <= Bounds.Height; j += _gridSize)
            {
                context.DrawLine(_gridPen, new Point(0, j), new Point(Bounds.Width, j));
            }
        }
        private void DrawStroke(Stroke stroke, double shakeIntensity, DrawingContext context)
        {
            if (!stroke.Shake)
                shakeIntensity = 0;

            for (int i = 1; i < stroke.Points.Count; i++)
            {
                SetupMainPen(stroke, i, shakeIntensity);
                var p1 = GetShakenPoint(stroke.Points[i - 1], shakeIntensity);
                var p2 = GetShakenPoint(stroke.Points[i], shakeIntensity);
                context.DrawLine(_mainPen, p1, p2);
            }
        }

        #endregion

        #region Brush Setup

        private void SetupMainPen(Stroke stroke, int i, double shakeIntensity)
        {
            float pressure = stroke.Pressures[i];
            double scaledPressure = Math.Min(pressure * 2, 1);
            var brush = ChooseBrushSettings(stroke, scaledPressure, pressure);
            _mainPen = brush;
            _mainPen.LineCap = stroke.PenLineCap;
        }

        private Pen ChooseBrushSettings(Stroke stroke, double scaledPressure, double rawPressure)
        {
            var color = stroke.Color switch
            {
                ColorType.First => Brushes.Black,
                ColorType.Second => Brushes.Blue,
                ColorType.Third => Brushes.Red,
                ColorType.Fourth => Brushes.White,
                ColorType.Fifth => Brushes.Yellow,
                ColorType.Sixth => Brushes.GreenYellow,
                ColorType.Seventh => Brushes.Purple,
                ColorType.Eighth => Brushes.Pink,
                _ => Brushes.Black
            };

            var size = stroke.Size switch
            {
                SizeType.Small => 2,
                SizeType.Medium => 8,
                SizeType.Large => 20,
                _ => 5
            };

            return new Pen(new SolidColorBrush(color.Color, stroke.Alpha * scaledPressure), size * rawPressure);
        }

        #endregion

        #region Public Methods

        public void ClearCanvas()
        {
            _currentStroke = null;
            _strokes.Clear();
            RequestInvalidate();
        }

        public void SelectColor(ColorType color) => _currentColor = color;
        public void SelectSize(SizeType size) => _currentSize = size;
        public void ChangeAlpha(double val) => _alpha = val;
        public void ChangeBrushTip(PenLineCap cap) => _currentCap = cap;
        public void ShouldShake(bool shake)
        {
            _isShake = shake;
            if (!_isShake)
            {
                InvalidateVisual();
            }
        }

        #endregion
    }
}