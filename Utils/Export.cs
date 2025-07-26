using System;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using ShakyDoodle.Controllers;
using System.Linq;
using Size = Avalonia.Size;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using ShakyDoodle.Services;
using ShakyDoodle.Models;

namespace ShakyDoodle.Utils
{
    public class Export
    {
        private readonly FrameController _frameController;
        private readonly FrameRendererService _frameRenderer;

        public Export(FrameController frameController, FrameRendererService frameRenderer)
        {
            _frameController = frameController;
            _frameRenderer = frameRenderer;
        }

        public void ExportFramesAsPng(string folderPath, int width, int height, BGType bg, bool isForGif = false)
        {
            var pixelSize = new PixelSize(width, height);
            string sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var frames = _frameController.GetAllFrames();
            
            int shakingFramesPerFrame = isForGif ? 12 : 1;
            double timeStep = Math.PI / 6;

            for (int i = 0; i < frames.Count; i++)
            {
                for (int shakeFrame = 0; shakeFrame < (isForGif && frames.Count == 1 ? shakingFramesPerFrame : 1); shakeFrame++)
                {
                    using var renderTarget = new RenderTargetBitmap(pixelSize);
                    using (var context = renderTarget.CreateDrawingContext(true))
                    {
                        double time = isForGif ? shakeFrame * timeStep : 0;
                        _frameRenderer.RenderFrame(context, frames[i], bg, time);
                    }

                    string fileName = isForGif ?
                        $"frame_{sessionId}_{i:D3}_{shakeFrame:D3}.png" :
                        $"frame_{sessionId}_{i:D3}.png";
                    string filePath = Path.Combine(folderPath, fileName);
                    renderTarget.Save(filePath);
                }
            }
        }

        public void ExportAsGif(string filePath, int width, int height, BGType bg, int frameDelay = 150)
        {
            var frames = _frameController.GetAllFrames();
            int adjustedFrameDelay = frames.Count == 1 ? 50 : frameDelay;

            var tempDir = Path.Combine(Path.GetTempPath(), "ShakyDoodle_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(tempDir);

            try
            {
                ExportFramesAsPng(tempDir, width, height, bg, true);

                var frameFiles = Directory.GetFiles(tempDir, "*.png").OrderBy(f => f).ToList();
                if (frameFiles.Count == 0) return;

                using var gif = Image.Load<Rgba32>(frameFiles[0]);
                gif.Metadata.GetGifMetadata().RepeatCount = 0;
                var gifFrameMetadata = gif.Frames.RootFrame.Metadata.GetGifMetadata();
                gifFrameMetadata.FrameDelay = adjustedFrameDelay / 10;

                foreach (var frameFile in frameFiles.Skip(1))
                {
                    using var frameImage = Image.Load<Rgba32>(frameFile);
                    var frameMetadata = frameImage.Frames.RootFrame.Metadata.GetGifMetadata();
                    frameMetadata.FrameDelay = adjustedFrameDelay / 10;
                    gif.Frames.AddFrame(frameImage.Frames.RootFrame);
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
