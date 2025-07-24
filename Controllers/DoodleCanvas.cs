
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
        public InputHandler InputHandler;
        public FrameController FrameController;


        private bool _lightbox = false;
        private bool _isLogo;
        private bool _isNoise = true;

        private BGType _currentBG = BGType.Grid;
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
            InputHandler = new InputHandler();
            _helper = new(this);
            _strokeRenderer = new(Bounds, _helper, InputHandler);
            FrameController = new(Bounds, _strokeRenderer);
            _shortcutHelper = new(FrameController);
            _exportHelper = new(Bounds, FrameController, _strokeRenderer);

            InputHandler.Initialize(FrameController, _shortcutHelper);

            FrameController.AddEmptyFrame();
            Focusable = true;
            PointerPressed += (s, e) =>
            {
                var point = e.GetPosition(this);
                InputHandler.PointerPressed(point, e.GetCurrentPoint(this).Properties.Pressure);
                _helper.RequestInvalidateThrottled();
            };
            PointerMoved += (s, e) =>
            {
                var point = e.GetPosition(this);
                InputHandler.PointerMoved(point, e.GetCurrentPoint(this).Properties.Pressure);
                _helper.RequestInvalidateThrottled();
            };
            PointerReleased += (s, e) =>
            {
                InputHandler.PointerReleased();
                _helper.RequestInvalidateThrottled();
            };
            FrameController.OnInvalidateRequested += () => _helper.RequestInvalidateThrottled();
            InputHandler.UpdateSettings(new Color(255, 0, 0, 0), SizeType.Small, 1,PenLineCap.Round, false);
            Cursor = new Cursor(StandardCursorType.Cross);

        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _strokeRenderer.StartRenderLoopAsync(() => FrameController.GetStrokes(), () => _shakeController.GetSpeed());
        }

        public override void Render(DrawingContext context) => _strokeRenderer.Render(context, _lightbox, FrameController.CurrentFrame, FrameController.GetAllVisibleStrokes(), FrameController.GetAllFrames(), Bounds, _isNoise, _currentBG);

        public void ClearCanvas()
        {
            FrameController.ClearAll();
            Stop();
            _helper.RequestInvalidateThrottled();
        }

        public void ChangeColor(Color color) => InputHandler.ChangeColor(color);
        public void SelectSize(SizeType size) => InputHandler.ChangeSize(size);
        public void ChangeAlpha(double val) => InputHandler.ChangeAlpha(val);
        public void ChangeBrushTip(PenLineCap cap) => InputHandler.ChangeCap(cap);
        public void ShouldShake(bool shake)
        {
            InputHandler.IsErasing = false;
            InputHandler.ChangeShake(shake);
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

        public void HandleUndo()
        {
            _shortcutHelper.Undo();
            _helper.RequestInvalidateThrottled();
        }

        public void HandleRedo()
        {
            _shortcutHelper.Redo();
            _helper.RequestInvalidateThrottled();
        }
        public void ToggleNoise() => _isNoise = _isNoise ? false : true;
        public void DuplicateFrame() => FrameController.DuplicateFrame(this);

        public void ExportFramesAsPng(string folderPath, int width, int height) => _exportHelper.ExportFramesAsPng(folderPath, width, height, _currentBG);
        public void ExportFramesAsGif(string folderPath, int width, int height) => _exportHelper.ExportAsGif(folderPath, width, height, _currentBG);
        public void NextLayer() => FrameController.SetCurrentLayer(FrameController.ActiveLayerIndex + 1);
        public void PrevLayer() => FrameController.SetCurrentLayer(FrameController.ActiveLayerIndex - 1);
        public void OnErase() => InputHandler.IsErasing = InputHandler.IsErasing ? false : true;
        public void OnDeleteLayer()
        {
            FrameController.DeleteCurrentLayer();
            _helper.RequestInvalidateThrottled();
        }

        public void DeleteCurrentFrame()
        {
            FrameController.DeleteCurrentFrame();
            _helper.RequestInvalidateThrottled();
        }
           
        public void ToggleBackground(BGType bg) => _currentBG = bg;
        public void OnUpdateOpacity(double val)
        {
            FrameController.UpdateLayerOpacity(val);
            FrameController.MarkDirty();
        }
        public double CurrentLayerOpacity() => FrameController.GetCurrentLayer()?.Opacity ?? 1.0;
    }
}
