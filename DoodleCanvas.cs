using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using SkiaSharp;
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
        Stroke? _currentStroke;

        ColorType _currentColor;
        SizeType _currentSize;
        int _gridSize;
        double _alpha;

        double _time;

        float _amp = 2;
        float _speed = 0.3f;
        int _maxStrokes;

        Pen _mainPen;
        PenLineCap _currentCap;

        bool _isShake = true;
        Pen _gridPen = new(Brushes.LightBlue, 1);
        private bool _needsRedraw = false;

        List<List<Stroke>> frames = new();
        int currentFrame = 0;
        private bool _onionSkinEnabled = false;
        public List<Stroke> Strokes => _strokes;
        private bool _isPlaying = false;

        private Stack<List<Stroke>> _undoStack = new();
        private Stack<List<Stroke>> _redoStack = new();
        #endregion

        #region Systems

        public DoodleCanvas()
        {
            _maxStrokes = 300;
            _strokes = new List<Stroke>(_maxStrokes);

            Focusable = true;
            PointerMoved += OnPointerMoved;
            PointerPressed += OnPointerPressed;
            PointerReleased += OnPointerReleased;

            frames = new List<List<Stroke>>() { new List<Stroke>() };
            currentFrame = 0;
            _gridSize = 50;
            _alpha = 1;
            _currentCap = PenLineCap.Square;

            Cursor = new Cursor(StandardCursorType.Cross);

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

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs events)
        {
            _currentStroke = null;
            SaveCurrentFrame();
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs events)
        {
            Focus();
            if (events.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _currentStroke = new(_currentColor, events.GetPosition(this), _currentSize, _alpha, _currentCap, events.GetCurrentPoint(this).Properties.Pressure, _isShake);
                PushUndoState();
                _strokes.Add(_currentStroke);
                System.Diagnostics.Debug.WriteLine($"Strokes: {_strokes.Count}");
                SyncStrokesToFrame();
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
                bool anyShakeStrokes = _strokes.Any(s => s.Shake);

                if (anyShakeStrokes)
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
            if (shakeIntensity <= 0) return point;

            double seedX = Math.Sin(point.X * 12.9898 + point.Y * 78.233);
            double seedY = Math.Cos(point.X * 93.9898 + point.Y * 67.345);

            double offsetX = (seedX * 43758.5453) % 2 - 1;
            double offsetY = (seedY * 12737.2349) % 2 - 1;

            double shakenX = point.X + Math.Sin(_time + offsetX * 10) * _amp * shakeIntensity;
            double shakenY = point.Y + Math.Cos(_time + offsetY * 10) * _amp * shakeIntensity;

            return new Point(shakenX, shakenY);
        }

        private void SaveCurrentFrame()
        {
            var strokesCopy = _strokes.Select(x => x.Clone()).ToList();
            if (currentFrame < frames.Count) frames[currentFrame] = strokesCopy;
            else frames.Add(strokesCopy);

        }
        void LoadFrame(int i)
        {
            if (i < 0 || i >= frames.Count) return;

            currentFrame = i;
            SyncFrameToStrokes(); 
            InvalidateVisual();
        }

        private double GetStrokeSize(Stroke stroke)
        {
            return stroke.Size switch
            {
                SizeType.Small => 2,
                SizeType.Medium => 8,
                SizeType.Large => 20,
                _ => 5
            };
        }
        public void SetStrokes(List<Stroke> strokes)
        {
            _strokes = strokes;
            InvalidateVisual();
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
        private void SyncStrokesToFrame()
        {
            if (currentFrame < 0 || currentFrame >= frames.Count) return;
            frames[currentFrame] = _strokes.Select(s => s.Clone()).ToList();
        }

        private void SyncFrameToStrokes()
        {
            if (currentFrame < 0 || currentFrame >= frames.Count) return;
            _strokes = frames[currentFrame].Select(s => s.Clone()).ToList();
        }
        private void PushUndoState()
        {
            _undoStack.Push(_strokes.Select(s => s.Clone()).ToList());
            _redoStack.Clear(); // Clear redo stack on new action
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
            if (_onionSkinEnabled)
            {
                DrawFrameIfExists(currentFrame - 1, Brushes.LightBlue, context); 
                DrawFrameIfExists(currentFrame + 1, Brushes.Pink, context); 
            }
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
        private void DrawStrokeWithColorOverride(Stroke stroke, double shakeIntensity, DrawingContext context, IBrush overrideColor)
        {
            if (!stroke.Shake) shakeIntensity = 0;

            var pen = new Pen(overrideColor, GetStrokeSize(stroke));
            pen.LineCap = stroke.PenLineCap;

            for (int i = 1; i < stroke.Points.Count; i++)
            {
                var p1 = GetShakenPoint(stroke.Points[i - 1], shakeIntensity);
                var p2 = GetShakenPoint(stroke.Points[i], shakeIntensity);
                context.DrawLine(pen, p1, p2);
            }
        }
        private void DrawFrameIfExists(int i, IBrush color, DrawingContext context)
        {
            //If our index is invalid we dont do nothin'
            if (i < 0 || i >= frames.Count) return;
            //We grab the frame 
            var f = frames[i];
            //Loop through its strokes
            foreach (var stroke in f)
            {
                DrawStrokeWithColorOverride(stroke, 0, context, color);
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
            frames.Clear();
            currentFrame = 0;

            frames.Add(new List<Stroke>());

            Stop(); 
            RequestInvalidate();

            if (_isShake)
                InvalidateVisual();
            else
                _needsRedraw = true;
        }

        public void SelectColor(ColorType color) => _currentColor = color;
        public void SelectSize(SizeType size) => _currentSize = size;
        public void ChangeAlpha(double val) => _alpha = val;
        public void ChangeBrushTip(PenLineCap cap) => _currentCap = cap;
        public void ShouldShake(bool shake)
        {
            _isShake = shake;
            _needsRedraw = true;
        }
        public void NextFrame()
        {
            SaveCurrentFrame();
            int n = currentFrame + 1;
            if (n >= frames.Count) frames.Add(new List<Stroke>());
            LoadFrame(n);
            InvalidateVisual();
        }
        public void PreviousFrame()
        {
            SaveCurrentFrame();
            if (currentFrame > 0) LoadFrame(currentFrame - 1);
            InvalidateVisual();
        }
        public async void Play()
        {
            if (frames.Count == 0) return;
            _isPlaying = true;
            while (_isPlaying)
            {
                LoadFrame(currentFrame);
                currentFrame = (currentFrame + 1) % frames.Count;

                await Task.Delay(100);
            }
        }
        public void Stop()
        {
            _isPlaying = false;
        }
        public void TogglePlay()
        {
            if (_isPlaying) Stop();
            else Play();
        }
        public void ToggleOnionSkin(bool enabled)
        {
            _onionSkinEnabled = enabled;
            _needsRedraw = true;
        }
        public void HandleUndo()
        {
            if (_undoStack.Count == 0) return;

            _redoStack.Push(_strokes.Select(s => s.Clone()).ToList());
            _strokes = _undoStack.Pop();
            SyncStrokesToFrame();
            InvalidateVisual();
        }
        public void HandleRedo()
        {
            if (_redoStack.Count == 0) return;

            _undoStack.Push(_strokes.Select(s => s.Clone()).ToList());
            _strokes = _redoStack.Pop();
            SyncStrokesToFrame();
            InvalidateVisual();
        }
        #endregion
    }
}