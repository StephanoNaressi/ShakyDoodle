using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using ShakyDoodle.Rendering;
using ShakyDoodle.Services;
using ShakyDoodle.Utils;
using ShakyDoodle.Views.Controls;

namespace ShakyDoodle.Controllers
{
    public class DoodleCanvas : Control
    {
        public static StyledProperty<double> CanvasWidthProperty = 
            AvaloniaProperty.Register<DoodleCanvas, double>(nameof(CanvasWidth), 1100);
            
        public static StyledProperty<double> CanvasHeightProperty = 
            AvaloniaProperty.Register<DoodleCanvas, double>(nameof(CanvasHeight), 1500);
            
        public double CanvasWidth
        {
            get => GetValue(CanvasWidthProperty);
            set => SetValue(CanvasWidthProperty, value);
        }
        
        public double CanvasHeight
        {
            get => GetValue(CanvasHeightProperty);
            set => SetValue(CanvasHeightProperty, value);
        }
        public bool IsPanning { get; set; }
        public bool IsSpacePressed { get; set; }
        public bool IsMirrored { get; set; }

        private readonly ShakeController _shakeController = new();
        private readonly LogoPreloader _logoPreloader = new();
        private readonly StrokeRenderer _strokeRenderer;
        private readonly AvaloniaExtras _helper;
        public readonly ShortcutHelper ShortcutHelper;
        private readonly Export _exportHelper;
        public readonly InputHandler InputHandler;
        public readonly FrameController FrameController;


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
            _helper = new(this);
            var dependencies = new DoodleDependencies(this, _helper);

            InputHandler = dependencies.InputHandler;
            _strokeRenderer = dependencies.StrokeRenderer;
            FrameController = dependencies.FrameController;
            ShortcutHelper = dependencies.ShortcutHelper;
            _exportHelper = dependencies.ExportHelper;

            InputHandler.Initialize(FrameController, ShortcutHelper);

            FrameController.AddEmptyFrame();
            Focusable = true;
            PointerPressed += (s, e) =>
            {
                if (IsPanning) return;
                var point = e.GetPosition(this);
                InputHandler.PointerPressed(point, e.GetCurrentPoint(this).Properties.Pressure);
                _helper.RequestInvalidateThrottled();
            };

            PointerMoved += (s, e) =>
            {
                if (IsPanning) return;
                var point = e.GetPosition(this);
                InputHandler.PointerMoved(point, e.GetCurrentPoint(this).Properties.Pressure);
                _helper.RequestInvalidateThrottled();
            };
            PointerReleased += (s, e) =>
            {
                if (IsPanning) return;
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

        public override void Render(DrawingContext context)
        {
            var canvasBounds = new Rect(0, 0, CanvasWidth, CanvasHeight);
            _strokeRenderer.Render(context, _lightbox, FrameController.CurrentFrame, 
                FrameController.GetAllVisibleStrokes(), FrameController.GetAllFrames(), 
                canvasBounds, _isNoise, _currentBG);
        }

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
        public void ChangeBrushType(BrushType brushType)
        {
            InputHandler.IsErasing = false;
            InputHandler.ChangeBrushType(brushType);
            _helper.RequestInvalidateThrottled();
        }
        public void ShouldShake(bool shake)
        {
            InputHandler.IsErasing = false;
            InputHandler.ChangeShake(shake);
            _helper.RequestInvalidateThrottled();
        }

        public void NextFrame() => FrameController.NextFrame();
        public void PreviousFrame() => FrameController.PreviousFrame();

        public void Play() => FrameController.Play();

        public void Stop() => FrameController.Stop();
        public void TogglePlay() => FrameController.TogglePlay();

        public void ToggleLightbox()
        {
            _lightbox = _lightbox ? false : true;
            _helper.RequestInvalidateThrottled();
        }

        public void HandleUndo()
        {
            ShortcutHelper.Undo();
            _helper.RequestInvalidateThrottled();
        }

        public void HandleRedo()
        {
            ShortcutHelper.Redo();
            _helper.RequestInvalidateThrottled();
        }
        public void ToggleNoise() => _isNoise = _isNoise ? false : true;
        public void DuplicateFrame() => FrameController.DuplicateFrame();

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
