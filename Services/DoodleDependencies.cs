using Avalonia;
using ShakyDoodle.Controllers;
using ShakyDoodle.Rendering;
using ShakyDoodle.Utils;

namespace ShakyDoodle.Services
{
    public class DoodleDependencies
    {
        public FrameController FrameController { get; }
        public StrokeRenderer StrokeRenderer { get; }
        public InputHandler InputHandler { get; }
        public ShortcutHelper ShortcutHelper { get; }
        public Export ExportHelper { get; }

        public DoodleDependencies(DoodleCanvas canvas, AvaloniaExtras helper)
        {
            InputHandler = new InputHandler();
            
            var strokeRenderer = new StrokeRenderer(canvas.Bounds, helper, InputHandler, canvas.CanvasWidth, canvas.CanvasHeight);
            
            var frameRendererService = new FrameRendererService(strokeRenderer, canvas.CanvasWidth, canvas.CanvasHeight);
            
            //strokeRenderer.SetFrameRendererService(frameRendererService);

            FrameController = new FrameController(canvas.Bounds, strokeRenderer);
            ShortcutHelper = new ShortcutHelper(FrameController);
            ExportHelper = new Export(FrameController, frameRendererService);
            
            StrokeRenderer = strokeRenderer;
        }
    }
}