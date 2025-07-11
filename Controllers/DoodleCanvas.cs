
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using ShakyDoodle.Rendering;
using ShakyDoodle.Utils;

namespace ShakyDoodle.Controllers
{
    public class DoodleCanvas : Control
    {
        private ShakeController _shakeController = new();
        private LogoPreloader _logoPreloader = new();
        private StrokeRenderer _strokeRenderer;
        private AvaloniaExtras _helper;
        private ShortcutHelper _shortcutHelper;
        private Export _exportHelper;
        private InputHandler _inputHandler;
        public FrameController FrameController;

        private bool _lightbox = false;
        private bool _isLogo;
        private bool _isEraser = false;
        public bool IsLogo
        {
            get => _isLogo;
            set
            {
                if (_isLogo == value) return;
                _isLogo = value;
                if (_isLogo)
                {
                    var logoStrokes = LogoStrokes.Get();
                    _logoPreloader.LoadPredefinedStrokes(FrameController, this);
                }
                else
                {
                    ClearCanvas();
                }
                InvalidateVisual();
            }
        }

        public DoodleCanvas()
        {
            _inputHandler = new InputHandler();
            _helper = new(this);
            _strokeRenderer = new(Bounds, _helper, _inputHandler);
            FrameController = new(Bounds, _strokeRenderer);
            _shortcutHelper = new(FrameController);
            _exportHelper = new(Bounds, FrameController, _strokeRenderer);

            _inputHandler.Initialize(FrameController, _shortcutHelper);

            FrameController.AddEmptyFrame();
            Focusable = true;
            PointerPressed += (s, e) =>
            {
                var point = e.GetPosition(this);
                _inputHandler.PointerPressed(point, e.GetCurrentPoint(this).Properties.Pressure);
                InvalidateVisual();
            };
            PointerMoved += (s, e) =>
            {
                var point = e.GetPosition(this);
                _inputHandler.PointerMoved(point, e.GetCurrentPoint(this).Properties.Pressure);
                InvalidateVisual();
            };
            PointerReleased += (s, e) =>
            {
                _inputHandler.PointerReleased();
                InvalidateVisual();
            };
            _inputHandler.UpdateSettings(new Color(255, 0, 0, 0), SizeType.Small, 1,PenLineCap.Round, false);
            Cursor = new Cursor(StandardCursorType.Cross);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _strokeRenderer.StartRenderLoopAsync(() => FrameController.GetStrokes(), () => _shakeController.GetSpeed());
        }

        public override void Render(DrawingContext context) => _strokeRenderer.Render(context, _lightbox, FrameController.CurrentFrame, FrameController.GetAllVisibleStrokes(), FrameController.GetAllFrames(), Bounds);

        public void ClearCanvas()
        {
            FrameController.ClearAll();
            Stop();
            _helper.RequestInvalidateThrottled();
        }

        public void ChangeColor(Color color) => _inputHandler.ChangeColor(color);
        public void SelectSize(SizeType size) => _inputHandler.ChangeSize(size);
        public void ChangeAlpha(double val) => _inputHandler.ChangeAlpha(val);
        public void ChangeBrushTip(PenLineCap cap) => _inputHandler.ChangeCap(cap);
        public void ShouldShake(bool shake)
        {
            _inputHandler.IsErasing = false;
            _inputHandler.ChangeShake(shake);
            _helper.RequestInvalidateThrottled();
        }

        public void NextFrame() => FrameController.NextFrame(this);
        public void PreviousFrame() => FrameController.PreviousFrame(this);

        public void Play() => FrameController.Play(this);

        public void Stop() => FrameController.Stop();
        public void TogglePlay() => FrameController.TogglePlay(this);

        public void ToggleLightbox()
        {
            _lightbox = _lightbox ? false : true;
            _helper.RequestInvalidateThrottled();
        }

        public void HandleUndo() => _shortcutHelper.Undo();

        public void HandleRedo() => _shortcutHelper.Redo();

        public void DuplicateFrame() => FrameController.DuplicateFrame(this);

        public void ExportFramesAsPng(string folderPath, int width, int height) => _exportHelper.ExportFramesAsPng(folderPath, width, height);
        public void NextLayer() => FrameController.SetCurrentLayer(FrameController.ActiveLayerIndex + 1);
        public void PrevLayer() => FrameController.SetCurrentLayer(FrameController.ActiveLayerIndex - 1);
        public void OnErase() => _inputHandler.IsErasing = _inputHandler.IsErasing ? false : true;
    }
}
