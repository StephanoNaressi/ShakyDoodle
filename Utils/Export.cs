using System;
using System.IO;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ShakyDoodle.Rendering;
using ShakyDoodle.Controllers;
using System.Linq;
using Size = Avalonia.Size;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;

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
            var size = new Size(width, height);

            for (int i = 0; i < frames.Count; i++)
            {
                using var renderTarget = new RenderTargetBitmap(pixelSize);
                using (var context = renderTarget.CreateDrawingContext(true))
                {
                    // Fill background
                    context.FillRectangle(Brushes.White, new Rect(0, 0, width, height));

                    // Draw grid
                    _strokeRenderer.DrawGrid(context, new Rect(0, 0, width, height), bg);

                    foreach (var layer in frames[i].Layers.Where(l => l.IsVisible))
                    {
                        var nonShakingStrokes = layer.Strokes.Where(s => !s.Shake).ToList();
                        if (nonShakingStrokes.Any())
                        {
                            using var layerBitmap = _strokeRenderer.RasterizeStrokes(nonShakingStrokes, size, layer.Opacity);
                            if (layerBitmap != null)
                            {
                                using (context.PushOpacity(layer.Opacity))
                                {
                                    context.DrawImage(
                                        layerBitmap,
                                        new Rect(0, 0, layerBitmap.Size.Width, layerBitmap.Size.Height),
                                        new Rect(0, 0, layerBitmap.Size.Width, layerBitmap.Size.Height));
                                }
                            }
                        }

                        // Then draw shaking strokes directly with layer opacity
                        foreach (var stroke in layer.Strokes.Where(s => s.Shake))
                        {
                            var shakeIntensity = stroke.Shake ? 1 : 0;
                            _strokeRenderer.DrawStroke(stroke, shakeIntensity, context, layer.Opacity);
                        }
                    }
                }

                string fileName = $"frame_{sessionId}_{i:D3}.png";
                string filePath = Path.Combine(folderPath, fileName);
                renderTarget.Save(filePath);
            }
        }

        public void ExportAsGif(string filePath, int width, int height, BGType bg, int frameDelay = 150)
        {
            var pixelSize = new PixelSize(width, height);
            var frames = _frameController.GetAllFrames();
            var size = new Size(width, height);

            // Create a temporary folder for our frame PNGs
            var tempDir = Path.Combine(Path.GetTempPath(), "ShakyDoodle_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(tempDir);

            try
            {
                ExportFramesAsPng(tempDir, width, height, bg);

                var frameFiles = Directory.GetFiles(tempDir, "*.png").OrderBy(f => f).ToList();
                
                using var gif = Image.Load<Rgba32>(frameFiles[0]);
                
                gif.Metadata.GetGifMetadata().RepeatCount = 0; // Loop forever
                gif.Frames[0].Metadata.GetGifMetadata().FrameDelay = frameDelay / 10;

                foreach (var frameFile in frameFiles.Skip(1))
                {
                    using var frameImage = Image.Load<Rgba32>(frameFile);
                    frameImage.Frames[0].Metadata.GetGifMetadata().FrameDelay = frameDelay / 10;
                    gif.Frames.AddFrame(frameImage.Frames[0]);
                }

                gif.SaveAsGif(filePath, new GifEncoder 
                { 
                    ColorTableMode = GifColorTableMode.Local
                });
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
    }
}
