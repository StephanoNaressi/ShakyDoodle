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
        public Stroke? MirroredStroke;
        public bool IsErasing { get; set; } = false;
        public double EraserRadius { get; set; } = 5.0;
        public bool SymmetryEnabled { get; set; } = false;
        private double _alpha;
        private Color _currentColor;
        private SizeType _currentSize;
        private PenLineCap _currentCap;
        private bool _isShake;
        private readonly List<Color> _recentColors = new();
        public List<Color> RecentColors => _recentColors;
        private bool _isClicking = false;
        private BrushType _currentBrushType = BrushType.Standard;
        private double _canvasWidth;
        private double _canvasHeight;
        public void Initialize(FrameController frameController, ShortcutHelper shortcutHelper)
        {
            for (int i = 0; i < 24; i++)
                _recentColors.Add(Colors.White);
            _frameController = frameController;
            _shortcutHelper = shortcutHelper;
        }
        public void UpdateCanvasSize(double width, double height)
        {
            _canvasWidth = width;
            _canvasHeight = height;
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
        private Point GetMirroredPoint(Point originalPoint)
        {
            double centerX = _canvasWidth / 2;
            double mirroredX = centerX + (centerX - originalPoint.X);
            return new Point(mirroredX, originalPoint.Y);
        }

        private Point ConstrainToStraightLine(Point start, Point current)
        {
            var dx = Math.Abs(current.X - start.X);
            var dy = Math.Abs(current.Y - start.Y);
            
            if (dx > dy * 2) // Horizontal
                return new Point(current.X, start.Y);
            else if (dy > dx * 2) // Vertical
                return new Point(start.X, current.Y);
            else // 45-degree diagonal
            {
                var distance = Math.Min(dx, dy);
                var signX = Math.Sign(current.X - start.X);
                var signY = Math.Sign(current.Y - start.Y);
                return new Point(start.X + distance * signX, start.Y + distance * signY);
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
            if (SymmetryEnabled)
            {
                var mirroredPosition = GetMirroredPoint(position);
                MirroredStroke = new Stroke(
                    _currentColor,
                    mirroredPosition,
                    _currentSize,
                    _alpha,
                    _currentCap,
                    pressure,
                    _isShake,
                    _currentBrushType
                );
            }
            var layer = _frameController.GetCurrentLayer();
            if (layer != null) layer.IsDirty = true;

        }

        public void PointerMoved(Point position, float pressure, bool isShiftPressed = false)
        {
            if (IsErasing && _isClicking)
            {
                var strokes = _frameController.GetStrokes();
                if (strokes == null)
                    return;

                _frameController.EraseStrokes(strokes, position, EraserRadius);

                if (SymmetryEnabled)
                {
                    var mirroredPosition = GetMirroredPoint(position);
                    _frameController.EraseStrokes(strokes, mirroredPosition, EraserRadius);
                }

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

            // Constrain position if Shift is held
            if (isShiftPressed && CurrentStroke.Points.Count > 0)
            {
                var startPoint = CurrentStroke.Points[0];
                position = ConstrainToStraightLine(startPoint, position);
            }

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

                    if (SymmetryEnabled && MirroredStroke != null)
                    {
                        var mirroredPoint = GetMirroredPoint(interpolatedPoint);
                        MirroredStroke.Points.Add(mirroredPoint);
                        MirroredStroke.Pressures.Add(clampedPressure);
                    }
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
            if (SymmetryEnabled && MirroredStroke != null)
            {
                strokes.Add(MirroredStroke);
            }
            CurrentStroke = null;
            MirroredStroke = null;
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
            MirroredStroke = null;
        }
        public void ToggleSymmetry()
        {
            SymmetryEnabled = !SymmetryEnabled;
        }
        public void ChangeSize(SizeType size) => _currentSize = size;
        public void ChangeCap(PenLineCap cap) => _currentCap = cap;

        public void ChangeBrushType(BrushType brushType)
        {
            _currentBrushType = brushType;
        }
    }
}
