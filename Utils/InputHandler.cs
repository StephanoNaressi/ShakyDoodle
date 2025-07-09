using System.Linq;
using Avalonia;
using Avalonia.Media;
using ShakyDoodle.Controllers;

namespace ShakyDoodle.Utils
{
    public class InputHandler
    {
        private FrameController _frameController;
        private ShortcutHelper _shortcutHelper;
        private MathExtras _mathHelper;
        private Stroke? _currentStroke;

        private double _alpha;
        private ColorType _currentColor;
        private SizeType _currentSize;
        private PenLineCap _currentCap;
        private bool _isShake;

        public InputHandler(FrameController frameController, ShortcutHelper shortcutHelper, MathExtras mathHelper)
        {
            _frameController = frameController;
            _shortcutHelper = shortcutHelper;
            _mathHelper = mathHelper;
        }

        public void UpdateSettings(ColorType color, SizeType size, double alpha, PenLineCap cap, bool isShake)
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
            _currentStroke = new(_currentColor, position, _currentSize, _alpha, _currentCap, pressure, _isShake);
            _shortcutHelper.PushUndoState(strokes);
            strokes.Add(_currentStroke);
            _frameController.SyncStrokesToFrame();
        }

        public void PointerMoved(Point position, float pressure)
        {
            if (_currentStroke == null) return;
            if (_currentStroke.Points.Count == 0 || _mathHelper.Distance(position, _currentStroke.Points.Last()) > 5)
            {
                _currentStroke.Points.Add(position);
                _currentStroke.Pressures.Add(pressure);
            }
        }

        public void PointerReleased()
        {
            _currentStroke = null;
            _frameController.SaveCurrentFrame();
        }
        public void ChangeShake(bool shake)
        {
            _isShake = shake;
        }
        public void ChangeAlpha(double val) => _alpha = val;
        public void ChangeColor(ColorType col) => _currentColor = col;
        public void ChangeSize(SizeType size) => _currentSize = size;
        public void ChangeCap(PenLineCap cap) => _currentCap = cap;
    }
}
