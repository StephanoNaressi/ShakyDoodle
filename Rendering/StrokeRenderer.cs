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
using ShakyDoodle.Utils.Brushes;

namespace ShakyDoodle.Rendering
{
    public class StrokeRenderer
    {
        private readonly double _canvasWidth;
        private readonly double _canvasHeight;
        private InputHandler _inputHandler;
        private ShakeController _shakeController = new();
        private BrushHelper _brushHelper = new();
        private AvaloniaExtras _helper;
        private int _gridSize = 50;
        private Pen _gridPen = new(new SolidColorBrush(Colors.LightBlue), 1);
        private Size _canvasSize;

        private RenderTargetBitmap? _prevFrameCache;
        private int _prevFrameCacheIndex = -1;
        private RenderTargetBitmap? _nextFrameCache;
        private int _nextFrameCacheIndex = -1;
        private RenderTargetBitmap? _noiseTexture;

        private StandardBrush _standardBrush = new();
        private ShakingBrush _shakingBrush;

        private Utils.Brushes.ImageBrush _acrylicBrush = new("Assets/acr_tip.png");
        private Utils.Brushes.ImageBrush _airbrush = new("Assets/air_tip.png");

        private RenderTargetBitmap? _activeStrokeCache;

        public StrokeRenderer(Rect bounds, AvaloniaExtras helper, InputHandler inputHandler, double canvasWidth, double canvasHeight)
        {
            _helper = helper;
            _inputHandler = inputHandler;
            _shakingBrush = new ShakingBrush(_shakeController);
            _canvasWidth = canvasWidth;
            _canvasHeight = canvasHeight;
            _canvasSize = new Size(canvasWidth, canvasHeight);
        }

        public void Render(DrawingContext context, bool lightbox, int currentFrame, List<Stroke> strokes, List<Frame> frames, Rect bounds, bool noise, BGType bg)
        {
            using var clip = context.PushClip(new Rect(bounds.Size));
            context.FillRectangle(Brushes.White, new Rect(bounds.Size));
            DrawGrid(context, new Rect(bounds.Size), bg);

            if (!IsValidFrame(currentFrame, frames))
                return;

            var frame = frames[currentFrame];

            if (lightbox)
            {
                DrawFrameCache(context, frames, currentFrame - 1, Brushes.LightBlue, ref _prevFrameCache, ref _prevFrameCacheIndex, bounds);
                DrawFrameCache(context, frames, currentFrame + 1, Brushes.Pink, ref _nextFrameCache, ref _nextFrameCacheIndex, bounds);
            }

            foreach (var layer in frame.Layers.Where(l => l.IsVisible))
            {
                var nonShakingStrokes = layer.Strokes.Where(s => !s.Shake).ToList();
                if (layer.IsDirty || layer.CachedBitmap == null)
                {
                    layer.CachedBitmap = RasterizeStrokes(nonShakingStrokes, _canvasSize);
                    layer.IsDirty = false;
                }
                if (layer.CachedBitmap != null)
                {
                    using (context.PushOpacity(layer.Opacity))
                    {
                        context.DrawImage(
                            layer.CachedBitmap,
                            new Rect(0, 0, _canvasWidth, _canvasHeight),
                            new Rect(0, 0, _canvasWidth, _canvasHeight));
                    }
                }
            }

            foreach (var layer in frame.Layers.Where(l => l.IsVisible))
            {
                double layerOpacity = layer.Opacity;
                foreach (var stroke in layer.Strokes.Where(s => s.Shake))
                {
                    if (stroke == _inputHandler.CurrentStroke)
                        continue;

                    double shakeIntensity = _shakeController.GetShakeIntensity(0, new() { stroke }, 1);
                    DrawStroke(stroke, shakeIntensity, context, layerOpacity);
                }
            }

            var activeStroke = _inputHandler.CurrentStroke;
            if (activeStroke != null)
            {
                if (_activeStrokeCache == null ||
                    _activeStrokeCache.PixelSize.Width != (int)_canvasSize.Width ||
                    _activeStrokeCache.PixelSize.Height != (int)_canvasSize.Height)
                {
                    _activeStrokeCache = new RenderTargetBitmap(new PixelSize((int)_canvasSize.Width, (int)_canvasSize.Height));
                }
                using (var ctx = _activeStrokeCache.CreateDrawingContext(true))
                {
                    ctx.FillRectangle(Brushes.Transparent, new Rect(0, 0, _activeStrokeCache.Size.Width, _activeStrokeCache.Size.Height));
                    DrawStroke(activeStroke, activeStroke.Shake ? _shakeController.GetShakeIntensity(0, new() { activeStroke }, 1) : 0, ctx, 1.0);
                }
                context.DrawImage(
                    _activeStrokeCache,
                    new Rect(0, 0, _activeStrokeCache.Size.Width, _activeStrokeCache.Size.Height),
                    new Rect(0, 0, _canvasWidth, _canvasHeight)
                );
            }

            if (noise) DrawNoise(context, bounds);
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
        public void DrawGrid(DrawingContext context, Rect bounds, BGType bg)
        {
            switch (bg)
            {
                case BGType.Lines:
                    for (int j = 0; j <= bounds.Height; j += _gridSize)
                    {
                        context.DrawLine(_gridPen, new Point(0, j), new Point(bounds.Width, j));
                    }
                    break;
                case BGType.Grid:
                    for (int i = 0; i <= bounds.Width; i += _gridSize)
                    {
                        context.DrawLine(_gridPen, new Point(i, 0), new Point(i, bounds.Height));
                    }
                    for (int j = 0; j <= bounds.Height; j += _gridSize)
                    {
                        context.DrawLine(_gridPen, new Point(0, j), new Point(bounds.Width, j));
                    }
                    break;
                case BGType.Blank:
                    break;
            }

        }
        public void DrawStroke(Stroke stroke, double shakeIntensity, DrawingContext context, double layerOpacity = 1.0)
        {
            switch (stroke.BrushType)
            {
                case BrushType.Standard:
                    _standardBrush.DrawStroke(stroke, context, layerOpacity, shakeIntensity);
                    break;
                case BrushType.Shaking:
                    _shakingBrush.DrawStroke(stroke, context, layerOpacity, shakeIntensity);
                    break;
                case BrushType.Acrylic:
                    _acrylicBrush.DrawStroke(stroke, context, layerOpacity);
                    break;
                case BrushType.Airbrush:
                    _airbrush.DrawStroke(stroke, context, layerOpacity);
                    break;
                case BrushType.Lasso:
                    if (stroke.Points.Count > 2)
                    {
                        var fillBrush = new SolidColorBrush(stroke.Color, stroke.Alpha * layerOpacity);
                        var geometry = new PathGeometry();
                        var figure = new PathFigure { IsClosed = true, StartPoint = stroke.Points[0] };
                        figure.Segments.Add(new PolyLineSegment(stroke.Points.Skip(1)));
                        geometry.Figures.Add(figure);
                        context.DrawGeometry(fillBrush, null, geometry);
                    }
                    break;
            }
        }

        public void DrawStrokeWithColorOverride(Stroke stroke, double shakeIntensity, Avalonia.Media.IBrush overrideColor, DrawingContext context)
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
        private RenderTargetBitmap RenderFrameToBitmap(Frame frame, Avalonia.Media.IBrush overrideColor, Size size)
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
        private void DrawFrameCache(DrawingContext context, List<Frame> frames, int frameIndex, Avalonia.Media.IBrush color, ref RenderTargetBitmap? cache, ref int cacheIndex, Rect bounds)
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
        public RenderTargetBitmap RasterizeStrokes(List<Stroke> strokes, Size size, double layerOpacity = 1.0)
        {
            if (strokes.Count == 0)
                return null;

            var pixelSize = new PixelSize((int)size.Width, (int)size.Height);
            var target = new RenderTargetBitmap(pixelSize);

            using (var ctx = target.CreateDrawingContext(true))
            {
                foreach (var stroke in strokes)
                {
                    DrawStroke(stroke, 0, ctx, layerOpacity);
                }
            }
            return target;
        }
        public void UpdateShakeTime(double time)
        {
            _shakeController.SetTime(time);
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
