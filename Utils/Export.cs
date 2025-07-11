using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ShakyDoodle.Rendering;
using ShakyDoodle.Models;
using ShakyDoodle.Controllers;
using System.Linq;

namespace ShakyDoodle.Utils
{
    public class Export
    {
        private readonly FrameController _frameController;
        private readonly StrokeRenderer _strokeRenderer;
        private readonly ColorTools _colorHelper = new();
        private readonly Rect _bounds;

        public Export(Rect bounds, FrameController frameController, StrokeRenderer strokeRenderer)
        {
            _frameController = frameController;
            _strokeRenderer = strokeRenderer;
            _bounds = bounds;

        }

        public void ExportFramesAsPng(string folderPath, int width, int height, BGType bg)
        {
            var pixelSize = new PixelSize(width, height);
            string sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var frames = _frameController.GetAllFrames();

            for (int i = 0; i < frames.Count; i++)
            {
                var frame = frames[i];
                using var renderTarget = new RenderTargetBitmap(pixelSize);
                using (var context = renderTarget.CreateDrawingContext(true))
                {
                    // Fill background
                    context.FillRectangle(Brushes.White, new Rect(0, 0, width, height));

                    // Draw grid
                    _strokeRenderer.DrawGrid(context, new Rect(0, 0, width, height), bg);
                    //Add layer handling
                    foreach (var layer in frame.Layers.Where(l => l.IsVisible))
                    {
                        foreach (var stroke in layer.Strokes)
                        {
                            var shakeIntensity = stroke.Shake ? 1 : 0;
                            var brush = new SolidColorBrush(stroke.Color);
                            _strokeRenderer.DrawStrokeWithColorOverride(stroke, shakeIntensity, brush, context);
                        }
                    }
                }

                // Save image
                string fileName = $"frame_{sessionId}_{i:D3}.png";
                string filePath = Path.Combine(folderPath, fileName);
                using var fileStream = File.OpenWrite(filePath);
                renderTarget.Save(fileStream);
            }
        }
    }
}
