using Avalonia.Media;
using Avalonia;
using System.Collections.Generic;
using ShakyDoodle.Utils;
using ShakyDoodle.Models;
using System.Threading.Tasks;
using System.Linq;
using System;
using ShakyDoodle.Controllers;
using Avalonia.Media.Imaging;

namespace ShakyDoodle.Rendering
{
    public class StrokeRenderer
    {
        private InputHandler _inputHandler;
        private ShakeController _shakeController = new();
        private BrushHelper _brushHelper = new();
        private AvaloniaExtras _helper;
        private int _gridSize = 50;
        private Pen _gridPen = new(Brushes.LightBlue, 1);
        int _maxStrokes = 150;

        private RenderTargetBitmap? _prevFrameCache;
        private int _prevFrameCacheIndex = -1;

        private RenderTargetBitmap? _nextFrameCache;
        private int _nextFrameCacheIndex = -1;
        private Size _canvasSize = new Size(0, 0);
        private RenderTargetBitmap? _noiseTexture;

        public StrokeRenderer(Rect bounds, AvaloniaExtras helper, InputHandler inputHandler)
        {
            _helper = helper;
            _inputHandler = inputHandler;
        }
        public void Render(DrawingContext context, bool lightbox, int currentFrame, List<Stroke> strokes, List<Frame> frames, Rect bounds, bool noise)
        {
            var newSize = new Size(
                Math.Max(_canvasSize.Width, bounds.Width),
                Math.Max(_canvasSize.Height, bounds.Height)
            );
            if (_canvasSize != newSize)
            {
                _canvasSize = newSize;

                if (IsValidFrame(currentFrame, frames))
                {
                    frames[currentFrame].CachedBitmap = null;
                    frames[currentFrame].IsDirty = true;
                }

                _prevFrameCache = null;
                _nextFrameCache = null;
                _prevFrameCacheIndex = -1;
                _nextFrameCacheIndex = -1;
            }
            using var clip = context.PushClip(new Rect(bounds.Size));
            context.FillRectangle(Brushes.White, new Rect(bounds.Size));
            DrawGrid(context, bounds);

            if (!IsValidFrame(currentFrame, frames))
                return;

            var frame = frames[currentFrame];

            if (lightbox)
            {
                DrawFrameCache(context, frames, currentFrame - 1, Brushes.LightBlue, ref _prevFrameCache, ref _prevFrameCacheIndex, bounds);
                DrawFrameCache(context, frames, currentFrame + 1, Brushes.Pink, ref _nextFrameCache, ref _nextFrameCacheIndex, bounds);
            }

            if (frame.CachedBitmap == null || frame.IsDirty)
            {
                var strokesToRasterize = frame.Layers.Where(l => l.IsVisible).SelectMany(l => l.Strokes).Where(s => !s.Shake).ToList();
                frame.CachedBitmap = RasterizeStrokes(strokesToRasterize, _canvasSize);
                frame.IsDirty = false;
            }

            if (frame.CachedBitmap != null)
            {
                context.DrawImage(
                    frame.CachedBitmap,
                    new Rect(0, 0, frame.CachedBitmap.Size.Width, frame.CachedBitmap.Size.Height),
                    new Rect(bounds.X, bounds.Y, frame.CachedBitmap.Size.Width, frame.CachedBitmap.Size.Height)
                );
            }
            var activeStroke = _inputHandler.CurrentStroke;
            using (context.PushTransform(Matrix.CreateTranslation(bounds.X, bounds.Y)))
            {
                foreach (var layer in frame.Layers.Where(l => l.IsVisible))
                {
                    foreach (var stroke in layer.Strokes.Where(s => s.Shake))
                    {
                        if (stroke == activeStroke) continue;
                        double shakeIntensity = _shakeController.GetShakeIntensity(0, new() { stroke }, 1);
                        DrawStroke(stroke, shakeIntensity, context);
                    }
                }

                if (activeStroke != null)
                {
                    double shakeIntensity = activeStroke.Shake ? _shakeController.GetShakeIntensity(0, new() { activeStroke }, 1) : 0;
                    DrawStroke(activeStroke, shakeIntensity, context);
                }
            }
            if(noise) DrawNoise(context, bounds);
        }

        private void DrawNoise(DrawingContext context, Rect bounds)
        {
            if (_noiseTexture == null ||
                _noiseTexture.PixelSize.Width < (int)bounds.Width ||
                _noiseTexture.PixelSize.Height < (int)bounds.Height)
            {
                _noiseTexture = ColorTools.Instance.GenerateNoiseTexture((int)_canvasSize.Width, (int)_canvasSize.Height, 30);
            }

            var options = new RenderOptions()
            {
                BitmapBlendingMode = BitmapBlendingMode.Multiply
            };

            using (context.PushRenderOptions(options))
            {
                context.DrawImage(
                    _noiseTexture,
                    new Rect(0, 0, _noiseTexture.Size.Width, _noiseTexture.Size.Height),
                    new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height)
                );
            }
        }
        public void DrawGrid(DrawingContext context, Rect bounds)
        {
            for (int i = 0; i <= bounds.Width; i += _gridSize)
            {
                context.DrawLine(_gridPen, new Point(i, 0), new Point(i, bounds.Height));
            }
            for (int j = 0; j <= bounds.Height; j += _gridSize)
            {
                context.DrawLine(_gridPen, new Point(0, j), new Point(bounds.Width, j));
            }
        }
        private void DrawStroke(Stroke stroke, double shakeIntensity, DrawingContext context)
        {
            if (stroke.Shake)
            {
                for (int i = 1; i < stroke.Points.Count; i++)
                {
                    var pen = _brushHelper.SetupMainPen(stroke, i, shakeIntensity);
                    var p1 = _shakeController.GetShakenPoint(stroke.Points[i - 1], shakeIntensity);
                    var p2 = _shakeController.GetShakenPoint(stroke.Points[i], shakeIntensity);
                    context.DrawLine(pen, p1, p2);
                }
            }
            else
            {
                var strokeBrush = _brushHelper.GetSolidBrush(stroke.Color, stroke.Alpha);

                for (int i = 1; i < stroke.Points.Count; i++)
                {
                    var pt = stroke.Points[i - 1];
                    float pressure = i < stroke.Pressures.Count ? stroke.Pressures[i] : 1f;

                    double size = _brushHelper.GetStrokeSize(stroke) * pressure;

                    switch (stroke.PenLineCap)
                    {
                        case PenLineCap.Round:
                            context.DrawEllipse(strokeBrush, null, pt, size / 2, size / 2);
                            break;

                        case PenLineCap.Square:
                            context.DrawRectangle(strokeBrush, null,
                            new Rect(pt.X - size / 2, pt.Y - size / 2, size, size));
                            break;
                        case PenLineCap.Flat:
                            context.DrawRectangle(strokeBrush, null,
                                new Rect(pt.X - size / 2, pt.Y - size / 2, size / 2, size));
                            break;

                        default:
                            context.DrawEllipse(strokeBrush, null, pt, size / 2, size / 2);
                            break;
                    }
                }
            }
        }

        public void DrawStrokeWithColorOverride(Stroke stroke, double shakeIntensity, IBrush overrideColor, DrawingContext context)
        {
            if (!stroke.Shake) shakeIntensity = 0;

            for (int i = 1; i < stroke.Points.Count; i++)
            {
                var pen = _brushHelper.ChooseBrushSettings(stroke, overrideColor, i < stroke.Pressures.Count ? stroke.Pressures[i] : 1f);
                pen.LineCap = stroke.PenLineCap;

                var p1 = _shakeController.GetShakenPoint(stroke.Points[i - 1], shakeIntensity);
                var p2 = _shakeController.GetShakenPoint(stroke.Points[i], shakeIntensity);
                context.DrawLine(pen, p1, p2);
            }
        }

        public async void StartRenderLoopAsync(Func<List<Stroke>> getStrokes, Func<float> getSpeed)
        {
            while (true)
            {
                _shakeController.UpdateTime(getSpeed());
                _helper.RequestInvalidateThrottled();
                await Task.Delay(16);
            }
        }
        private RenderTargetBitmap RenderFrameToBitmap(Frame frame, IBrush overrideColor, Size size)
        {
            var pixelSize = new PixelSize((int)size.Width, (int)size.Height);
            var renderTarget = new RenderTargetBitmap(pixelSize);

            using (var ctx = renderTarget.CreateDrawingContext(true))
            {
                foreach (var layer in frame.Layers.Where(l => l.IsVisible))
                {
                    ctx.FillRectangle(Brushes.Transparent, new Rect(size));
                    foreach (var stroke in layer.Strokes)
                    {
                        DrawStrokeWithColorOverride(stroke, 0, overrideColor, ctx);
                    }
                }

            }
            return renderTarget;
        }
        private void DrawFrameCache(DrawingContext context, List<Frame> frames, int frameIndex, IBrush color, ref RenderTargetBitmap? cache, ref int cacheIndex, Rect bounds)
        {
            if (!IsValidFrame(frameIndex, frames))
                return;

            if (cache == null || cacheIndex != frameIndex)
            {
                cache = RenderFrameToBitmap(frames[frameIndex], color, _canvasSize);
                cacheIndex = frameIndex;
            }

            if (cache != null)
            {
                context.DrawImage(cache,
                    new Rect(0, 0, cache.Size.Width, cache.Size.Height),
                    new Rect(bounds.X, bounds.Y, cache.Size.Width, cache.Size.Height));
            }
        }
        private RenderTargetBitmap RasterizeStrokes(List<Stroke> strokes, Size size)
        {
            var pixelSize = new PixelSize((int)size.Width, (int)size.Height);
            var target = new RenderTargetBitmap(pixelSize);

            using (var ctx = target.CreateDrawingContext(true))
            {
                foreach (var stroke in strokes)
                {
                    DrawStroke(stroke, 0, ctx);
                }
            }
            return target;
        }
        public void ClearCaches()
        {
            _prevFrameCache = null;
            _nextFrameCache = null;
            _prevFrameCacheIndex = -1;
            _nextFrameCacheIndex = -1;
        }
        private bool IsValidFrame(int index, List<Frame> frames) => index >= 0 && index < frames.Count;
    }
}
