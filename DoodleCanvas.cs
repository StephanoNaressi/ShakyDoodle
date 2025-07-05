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

        List<Stroke> _strokes = new();
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



        #endregion

        #region Systems

        public DoodleCanvas()
        {
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

            _ = new PointerPointProperties();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            StartRenderLoopAsync();
        }

        private async void RequestInvalidate()
        {
            if (_isInvalidating) return;

            _isInvalidating = true;
            InvalidateVisual();
            await Task.Delay(16);
            _isInvalidating = false;
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
                _currentStroke = new(_currentColor, events.GetPosition(this), _currentSize, _alpha, _currentCap, events.GetCurrentPoint(this).Properties.Pressure);
                _strokes.Add(_currentStroke);
                System.Diagnostics.Debug.WriteLine($"Strokes: {_strokes.Count}");
                RequestInvalidate();
            }
        }

        private void OnPointerMoved(object? sender, PointerEventArgs events)
        {
            if (_currentStroke == null || !events.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

            _currentStroke.Points.Add(events.GetPosition(this));
            _currentStroke.Pressures.Add(events.GetCurrentPoint(this).Properties.Pressure);
            RequestInvalidate();
        }

        private async void StartRenderLoopAsync()
        {
            while (true)
            {
                if (_isShake)
                {
                    _time += _speed;
                    RequestInvalidate();
                }
                await Task.Delay(16);
            }
        }

        #endregion

        #region Utility

        private Point GetShakenPoint(Point point, double shakeIntensity)
        {
            if (_shakeSeeds.Count > 10000)
                _shakeSeeds.Clear();

            if (!_isShake || shakeIntensity <= 0) return point;

            if (!_shakeSeeds.ContainsKey(point))
            {
                _shakeSeeds[point] = new Vector(_r.NextDouble() * _offset, _r.NextDouble() * _offset);
            }

            var seed = _shakeSeeds[point];
            return new Point(point.X + Math.Sin(_time + seed.X) * _amp * shakeIntensity,
                point.Y + Math.Cos(_time + seed.Y) * _amp * shakeIntensity);
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

            context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));

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
            Pen gridPen = new(Brushes.LightBlue, 1);
            for (int i = 0; i <= Bounds.Width; i += _gridSize)
            {
                context.DrawLine(gridPen, new Point(i, 0), new Point(i, Bounds.Height));
            }
            for (int j = 0; j <= Bounds.Height; j += _gridSize)
            {
                context.DrawLine(gridPen, new Point(0, j), new Point(Bounds.Width, j));
            }
        }
        private void DrawStroke(Stroke stroke, double shakeIntensity, DrawingContext context)
        {
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
            double scaledPressure = Math.Clamp(pressure / 0.5, 0, 1);
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
        public void ShouldShake(bool shake) => _isShake = shake;

        #endregion
    }
}