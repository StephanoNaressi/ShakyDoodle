using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Skia; 
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.IO;

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

        public DoodleCanvas()
        {
            PointerMoved += OnPointerMoved;
            PointerPressed += OnPointerPressed;
            PointerReleased += OnPointerReleased;
            _gridSize = 50;
            _alpha = 1;
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs events) => _currentStroke = null;

        private void OnPointerPressed(object? sender, PointerPressedEventArgs events)
        {
            if (events.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _currentStroke = new(_currentColor, events.GetPosition(this), _currentSize, _alpha);
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
                Pen pen = new(brush, size);
                pen.LineCap = PenLineCap.Square;


                //Draw lines with our strokes
                for (int i = 1; i < stroke.Points.Count; i++)
                {
                    context.DrawLine(pen, stroke.Points[i - 1], stroke.Points[i]);
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

        public void ChangeAlpha(double val)
        {
            _alpha = val;
        }
    }
}
