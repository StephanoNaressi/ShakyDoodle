using System;
using System.Collections.Generic;
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
        private BrushType _currentBrushType = BrushType.Standard;

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

            if (!_recentColors.Any(c => c.Equals(color)))
            {
                _recentColors.Insert(0, color);
                if (_recentColors.Count > 20)
                    _recentColors.RemoveAt(_recentColors.Count - 1);
            }
        }

        public void PointerPressed(Point position, float pressure)
        {
            _isClicking = true;
            var strokes = _frameController.GetStrokes();
            _shortcutHelper.PushUndoState(strokes.Select(s => s.Clone()).ToList());

            CurrentStroke = new Stroke(
                _currentColor,
                position,
                _currentSize,
                _alpha,
                _currentCap,
                pressure,
                _isShake,
                _currentBrushType
            );
            var layer = _frameController.GetCurrentLayer();
            if (layer != null) layer.IsDirty = true;

        }

        public void PointerMoved(Point position, float pressure)
        {
            if (IsErasing && _isClicking)
            {
                var strokes = _frameController.GetStrokes();
                if (strokes == null)
                    return;

                _frameController.EraseStrokes(strokes, position, EraserRadius);
                var frames = _frameController.GetAllFrames();
                if (_frameController.CurrentFrame >= 0 && _frameController.CurrentFrame < frames.Count)
                {
                    frames[_frameController.CurrentFrame].IsDirty = true;
                    var layer = _frameController.GetCurrentLayer();
                    if (layer != null)
                        layer.IsDirty = true;
                    frames[_frameController.CurrentFrame].CachedBitmap = null;
                }
                _frameController.MarkDirty();
                return;
            }
            if (CurrentStroke == null)
                return;

            float clampedPressure = Math.Max(pressure, 0.1f);

            float spacing = _currentBrushType == BrushType.Acrylic ? GetSpacingForSize(_currentSize) : (_isShake ? 5f : 2f);

            if (CurrentStroke.Points.Count < 1)
            {
                CurrentStroke.Points.Add(position);
                CurrentStroke.Pressures.Add(clampedPressure);
                return;
            }

            var lastPoint = CurrentStroke.Points.Last();
            double gap = MathExtras.Instance.Distance(position, lastPoint);

            if (gap >= spacing)
            {
                var direction = position - lastPoint;
                var normalizedDirection = direction / gap;

                int pointsToadd = (int)(gap / spacing);
                for (int i = 1; i <= pointsToadd; i++)
                {
                    var interpolatedPoint = lastPoint + normalizedDirection * (i * spacing);
                    CurrentStroke.Points.Add(interpolatedPoint);
                    CurrentStroke.Pressures.Add(clampedPressure);
                }
            }
        }

        private float GetSpacingForSize(SizeType size)
        {
            return size switch
            {
                SizeType.Small => 4f,
                SizeType.Medium => 6f,
                SizeType.Large => 10f,
                SizeType.ExtraLarge => 15f,
                _ => 4f
            };
        }

        public void PointerReleased()
        {
            _isClicking = false;
            _frameController.SaveCurrentFrame();
            var layer = _frameController.GetCurrentLayer();
            if (layer != null) layer.IsDirty = true;
            var strokes = _frameController.GetStrokes();
            if (CurrentStroke != null)
            {
                strokes.Add(CurrentStroke);
            }
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

        public void ChangeBrushType(BrushType brushType)
        {
            _currentBrushType = brushType;
        }
    }
}
