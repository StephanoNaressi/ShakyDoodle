using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Metsys.Bson;
using ShakyDoodle.Controllers;
using ShakyDoodle.Models;
using ShakyDoodle.Rendering;

namespace ShakyDoodle.Utils
{
    public class InputHandler
    {
        private FrameController _frameController;
        private ShortcutHelper _shortcutHelper;
        public Stroke? CurrentStroke;
        public bool IsErasing { get; set; } = false;
        public double EraserRadius { get; set; } = 5.0;

        private double _alpha;
        private Color _currentColor;
        private SizeType _currentSize;
        private PenLineCap _currentCap;
        private bool _isShake;
        private readonly List<Color> _recentColors = new();
        public List<Color> RecentColors => _recentColors;
        private bool _isClicking = false;
        public void Initialize(FrameController frameController, ShortcutHelper shortcutHelper)
        {
            for (int i = 0; i < 24; i++)
                _recentColors.Add(Colors.White);
            _frameController = frameController;
            _shortcutHelper = shortcutHelper;
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
            _isClicking = true;
            var strokes = _frameController.GetStrokes();
            _shortcutHelper.PushUndoState(strokes.Select(s => s.Clone()).ToList());
            CurrentStroke = new Stroke(_currentColor, position, _currentSize, _alpha, _currentCap, pressure, _isShake);
            strokes.Add(CurrentStroke);
            var layer = _frameController.GetCurrentLayer();
            if (layer != null) layer.IsDirty = true;

        }

        public void PointerMoved(Point position, float pressure)
        {
            if (IsErasing && _isClicking)
            {
                var strokes = _frameController.GetStrokes();
                if (strokes == null) return;

                _frameController.EraseStrokes(strokes, position, EraserRadius);

                var frames = _frameController.GetAllFrames();
                if (_frameController.CurrentFrame >= 0 && _frameController.CurrentFrame < frames.Count)
                {
                    frames[_frameController.CurrentFrame].IsDirty = true;
                    var layer = _frameController.GetCurrentLayer();
                    if (layer != null) layer.IsDirty = true;
                    frames[_frameController.CurrentFrame].CachedBitmap = null;
                }

                _frameController.MarkDirty();
                return;
            }

            if (CurrentStroke == null) return;

            float spacing = _isShake ? 5 : 1f;

            if (CurrentStroke.Points.Count == 0)
            {
                CurrentStroke.Points.Add(position);
                CurrentStroke.Pressures.Add(pressure);
                return;
            }

            var lastPoint = CurrentStroke.Points.Last();
            double dist = MathExtras.Instance.Distance(position, lastPoint);

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
            _isClicking = false;
            _frameController.SaveCurrentFrame();
            var layer = _frameController.GetCurrentLayer();
            if (layer != null) layer.IsDirty = true;
            CurrentStroke = null;
        }

        public void ChangeShake(bool shake)
        {
            _isShake = shake;
        }
        public void ChangeAlpha(double val) => _alpha = val;
        public void ChangeColor(Color col)
        {
            _currentColor = col;
            var isSimilar = ColorTools.Instance.AreColorsSimilar(_recentColors[0], col);
            if (_recentColors.Count == 0 || !isSimilar)
            {
                _recentColors.Remove(col);
                _recentColors.Insert(0, col);
                if (_recentColors.Count > 24)
                    _recentColors.RemoveAt(_recentColors.Count - 1);
            }
        }
        public void CancelStroke()
        {
            _isClicking = false;
            CurrentStroke = null;
        }

        public void ChangeSize(SizeType size) => _currentSize = size;
        public void ChangeCap(PenLineCap cap) => _currentCap = cap;
    }
}
