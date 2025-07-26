using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ShakyDoodle.Models;
using ShakyDoodle.Rendering;
using System.Linq;

namespace ShakyDoodle.Services
{
    public class FrameRendererService
    {
        private readonly StrokeRenderer _strokeRenderer;
        private readonly double _canvasWidth;
        private readonly double _canvasHeight;

        public FrameRendererService(StrokeRenderer strokeRenderer, double canvasWidth, double canvasHeight)
        {
            _strokeRenderer = strokeRenderer;
            _canvasWidth = canvasWidth;
            _canvasHeight = canvasHeight;
        }

        public void RenderFrame(DrawingContext context, Frame frame, BGType bg, double time = 0)
        {
            var bounds = new Rect(0, 0, _canvasWidth, _canvasHeight);
            context.FillRectangle(Brushes.White, bounds);
            _strokeRenderer.DrawGrid(context, bounds, bg);

            if (time > 0)
            {
                _strokeRenderer.UpdateShakeTime(time);
            }

            foreach (var layer in frame.Layers.Where(l => l.IsVisible))
            {
                var nonShakingStrokes = layer.Strokes.Where(s => !s.Shake).ToList();
                if (nonShakingStrokes.Any())
                {
                    var layerBitmap = _strokeRenderer.RasterizeStrokes(nonShakingStrokes, new Size(_canvasWidth, _canvasHeight));
                    if (layerBitmap != null)
                    {
                        using (context.PushOpacity(layer.Opacity))
                        {
                            context.DrawImage(layerBitmap, bounds, bounds);
                        }
                    }
                }

                foreach (var stroke in layer.Strokes.Where(s => s.Shake))
                {
                    _strokeRenderer.DrawStroke(stroke, 1.0, context, layer.Opacity);
                }
            }
        }
    }
}