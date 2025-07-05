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
        //Our drawn lines
        List<Stroke> _strokes = new();
        Stroke _currentStroke;

        //Current Color
        ColorType _currentColor;
        //Current Size
        SizeType _currentSize;
        int _gridSize;
        double _alpha;

        // Shake
        Random _r = new();
        double _time;

        //Shake values
        float _amp = 3;
        float _offset = 10;
        float _speed = 0.2f;

        Pen _mainPen;
        PenLineCap _currentCap;
        Dictionary<Point, Vector> _shakeSeeds = new();

        public DoodleCanvas()
        {
            PointerMoved += OnPointerMoved;
            PointerPressed += OnPointerPressed;
            PointerReleased += OnPointerReleased;
            _gridSize = 50;
            _alpha = 1;
            _currentCap = PenLineCap.Square;
        }

        private async void StartRenderLoopAsync()
        {
            while (true)
            {
                _time += _speed;
                InvalidateVisual();
                await Task.Delay(16);
            }

        }
        private Point GetShakenPoint(Point point)
        {
            //If the point is not in our dict then we assign a random double offset to it
            if (!_shakeSeeds.ContainsKey(point))
            {
                _shakeSeeds[point] = new Vector(_r.NextDouble() * _offset, _r.NextDouble() * _offset);
            }

            // We return our point with an offset added to a sine and cosine to add looping smooth transitions :P
            var seed = _shakeSeeds[point];
            return new Point(point.X + Math.Sin(_time + seed.X) * _amp,
                point.Y + Math.Cos(_time + seed.Y) * _amp);
        }
        protected override void OnInitialized()
        {
            base.OnInitialized();
            StartRenderLoopAsync();
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs events) => _currentStroke = null;

        private void OnPointerPressed(object? sender, PointerPressedEventArgs events)
        {
            if (events.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _currentStroke = new(_currentColor, events.GetPosition(this), _currentSize, _alpha, _currentCap);
                _strokes.Add(_currentStroke);
                //Updates the control so the canvas repaints
                InvalidateVisual();
            }
        }
        private void OnPointerMoved(object? sender, PointerEventArgs events)
        {
            //Dont draw if our current stroke is null
            //If Left button not pressed dont do shit
            if (_currentStroke == null || !events.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

            //Else we add the current stroke onto our list of strokes :p
            _currentStroke.Points.Add(events.GetPosition(this));

            //Updates the control so the canvas repaints
            InvalidateVisual();
        }
        
        public override void Render(DrawingContext context)
        {
            //PushPop to avoid drawing outside the canvas
            using var clip = context.PushClip(new Rect(Bounds.Size));

            base.Render(context);
            context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));

            //Grid

            Pen gridPen = new(Brushes.LightBlue, 1);

            for (int i = 0; i <= Bounds.Width; i += _gridSize)
            {
                context.DrawLine(gridPen, new Point(i, 0), new Point(i, Bounds.Height));
            }
            for (int j = 0; j <= Bounds.Height; j += _gridSize)
            {
                context.DrawLine(gridPen, new Point(0, j), new Point(Bounds.Width, j));
            }

            //For each of our current strokes stored
            foreach (var stroke in _strokes)
            {
                var color = stroke.Color switch
                {
                    ColorType.First => Brushes.Black,
                    ColorType.Second => Brushes.Blue,
                    ColorType.Third => Brushes.Red,
                    ColorType.Fourth => Brushes.White,
                    ColorType.Eraser => Brushes.Transparent,
                    _ => Brushes.Black
                };
                var size = stroke.Size switch
                {
                    SizeType.Small => 5,
                    SizeType.Medium => 10,
                    SizeType.Large => 15,
                    _ => 5
                };
                var brush = new SolidColorBrush(color.Color, stroke.Alpha);
                //Brush to paint with
                _mainPen = new(brush, size);
                _mainPen.LineCap = stroke.PenLineCap;


                //Draw lines with our strokes
                for (int i = 1; i < stroke.Points.Count; i++)
                {
                    var p1 = GetShakenPoint(stroke.Points[i - 1]);
                    var p2 = GetShakenPoint(stroke.Points[i]);
                    context.DrawLine(_mainPen, p1, p2);
                }
            }
        }

        public void ClearCanvas()
        {
            _strokes.Clear();
            InvalidateVisual();
        }
        public void SelectColor(ColorType color) => _currentColor = color;
        public void SelectSize(SizeType size) => _currentSize = size;
        public void ChangeAlpha(double val) => _alpha = val;
        public void ChangeBrushTip(PenLineCap cap)
        {
            _currentCap = cap;
        }
    }
}
