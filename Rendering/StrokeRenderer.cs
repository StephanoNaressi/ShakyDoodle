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

        public StrokeRenderer(Rect bounds, AvaloniaExtras helper)
        {
            _helper = helper;
        }
        public void Render(DrawingContext context, bool lightbox, int currentFrame, List<Stroke> strokes, List<Frame> frames, Rect bounds)
        {
            using var clip = context.PushClip(new Rect(bounds.Size));
            context.FillRectangle(Brushes.White, new Rect(bounds.Size));
            DrawGrid(context, bounds);

            if (lightbox)
            {
                DrawFrameCache(context, frames, currentFrame - 1, Brushes.LightBlue, ref _prevFrameCache, ref _prevFrameCacheIndex, bounds);
                DrawFrameCache(context, frames, currentFrame + 1, Brushes.Pink, ref _nextFrameCache, ref _nextFrameCacheIndex, bounds);
            }

            if (IsValidFrame(currentFrame, frames))
            {
                DrawVisibleLayers(context, frames[currentFrame]);
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
                                new Rect(pt.X - size / 2, pt.Y - size / 2, size/2, size));
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
                var strokes = getStrokes();
                bool anyShakeStrokes = strokes.Any(s => s.Shake);
                if (anyShakeStrokes)
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
        private void DrawVisibleLayers(DrawingContext context, Frame frame)
        {
            foreach (var layer in frame.Layers.Where(l => l.IsVisible))
            {
                foreach (var stroke in layer.Strokes)
                {
                    double shakeIntensity = stroke.Shake
                        ? _shakeController.GetShakeIntensity(layer.Strokes.IndexOf(stroke), layer.Strokes, _maxStrokes)
                        : 0;

                    DrawStroke(stroke, shakeIntensity, context);
                }
            }
        }
        private void DrawFrameCache(DrawingContext context, List<Frame> frames, int frameIndex, IBrush color, ref RenderTargetBitmap? cache, ref int cacheIndex, Rect bounds)
        {
            if (!IsValidFrame(frameIndex, frames))
                return;

            if (cache == null || cacheIndex != frameIndex)
            {
                cache = RenderFrameToBitmap(frames[frameIndex], color, bounds.Size);
                cacheIndex = frameIndex;
            }

            if (cache != null)
            {
                context.DrawImage(cache,
                    new Rect(0, 0, cache.Size.Width, cache.Size.Height),
                    new Rect(bounds.X, bounds.Y, cache.Size.Width, cache.Size.Height));
            }
        }
        private bool IsValidFrame(int index, List<Frame> frames) => index >= 0 && index < frames.Count;
    }
}
