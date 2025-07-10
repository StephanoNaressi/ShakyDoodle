using System;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using ShakyDoodle.Controllers;
using ShakyDoodle.Models;

namespace ShakyDoodle.Utils
{
    public class InputHandler
    {
        private FrameController _frameController;
        private ShortcutHelper _shortcutHelper;
        private MathExtras _mathHelper;
        public Stroke? CurrentStroke;

        private double _alpha;
        private Color _currentColor;
        private SizeType _currentSize;
        private PenLineCap _currentCap;
        private bool _isShake;
        public void Initialize(FrameController frameController, ShortcutHelper shortcutHelper, MathExtras mathHelper)
        {
            _frameController = frameController;
            _shortcutHelper = shortcutHelper;
            _mathHelper = mathHelper;
        }
        public void UpdateSettings(Color color, SizeType size, double alpha, PenLineCap cap, bool isShake)
        {
            _currentColor = color;
            _currentSize = size;
            _alpha = alpha;
            _currentCap = cap;
            _isShake = isShake;
        }

        public void PointerPressed(Point position, float pressure)
        {
            var strokes = _frameController.GetStrokes();
            CurrentStroke = new Stroke(_currentColor, position, _currentSize, _alpha, _currentCap, pressure, _isShake);
            strokes.Add(CurrentStroke);
            _shortcutHelper.PushUndoState(strokes.Select(s => s.Clone()).ToList());

        }

        public void PointerMoved(Point position, float pressure)
        {
            if (CurrentStroke == null) return;

            float spacing = _isShake ? 5 : 1f;

            if (CurrentStroke.Points.Count == 0)
            {
                CurrentStroke.Points.Add(position);
                CurrentStroke.Pressures.Add(pressure);
                return;
            }

            var lastPoint = CurrentStroke.Points.Last();
            double dist = _mathHelper.Distance(position, lastPoint);

            if (dist == 0) return;

            int amountToFill = Math.Max(1, (int)(dist / spacing));
            for (int i = 1; i < amountToFill; i++)
            {
                double t = (double)i / amountToFill;
                var interpolation = new Point(
                    lastPoint.X + (position.X - lastPoint.X) * t,
                    lastPoint.Y + (position.Y - lastPoint.Y) * t
                );

                CurrentStroke.Points.Add(interpolation);
                CurrentStroke.Pressures.Add(pressure);
            }

            CurrentStroke.Points.Add(position);
            CurrentStroke.Pressures.Add(pressure);

        }

        public void PointerReleased()
        {
            _frameController.SaveCurrentFrame();
            CurrentStroke = null;
        }

        public void ChangeShake(bool shake)
        {
            _isShake = shake;
        }
        public void ChangeAlpha(double val) => _alpha = val;
        public void ChangeColor(Color col) => _currentColor = col;
        public void ChangeSize(SizeType size) => _currentSize = size;
        public void ChangeCap(PenLineCap cap) => _currentCap = cap;
    }
}
