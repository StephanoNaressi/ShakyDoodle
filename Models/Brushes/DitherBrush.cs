using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ShakyDoodle.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Platform;

namespace ShakyDoodle.Models.Brushes
{
    public class DitherBrush : IBrush
    {
        private Bitmap _brushImage;
        private readonly Dictionary<uint, RenderTargetBitmap> _tintedCache = new();

        public DitherBrush(string imagePath)
        {
            var uri = new Uri($"avares://ShakyDoodle/Assets/{Path.GetFileName(imagePath)}");
            using var stream = AssetLoader.Open(uri);
            var original = new Bitmap(stream);

            double scaleFactor = 0.5;
            var newWidth = (int)(original.PixelSize.Width * scaleFactor);
            var newHeight = (int)(original.PixelSize.Height * scaleFactor);
            _brushImage = original.CreateScaledBitmap(new PixelSize(newWidth, newHeight));
        }

        public void DrawStroke(Stroke stroke, DrawingContext context, double layerOpacity = 1.0, double shakeIntensity = 0.0)
        {
            if (stroke.Points.Count < 2)
                return;

            var strokeAlpha = stroke.Alpha * layerOpacity;
            var tintedBrushBitmap = GetTintedBrush(stroke.Color);

            var imageBrush = new Avalonia.Media.ImageBrush(tintedBrushBitmap)
            {
                TileMode = TileMode.Tile,
                SourceRect = new RelativeRect(tintedBrushBitmap.Size, RelativeUnit.Absolute),
                DestinationRect = new RelativeRect(tintedBrushBitmap.Size, RelativeUnit.Absolute),
                Opacity = strokeAlpha
            };

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(stroke.Points[0], false);
                for (int i = 1; i < stroke.Points.Count; i++)
                {
                    ctx.LineTo(stroke.Points[i]);
                }
                ctx.EndFigure(false);
            }

            var pen = new Pen(
                brush: imageBrush,
                thickness: GetStrokeSize(stroke),
                lineCap: stroke.PenLineCap,
                lineJoin: PenLineJoin.Round
            );
            context.DrawGeometry(null, pen, geometry);
        }

        private RenderTargetBitmap GetTintedBrush(Color color)
        {
            uint key = (uint)color.A << 24 | (uint)color.R << 16 | (uint)color.G << 8 | color.B;
            if (_tintedCache.TryGetValue(key, out var cached))
                return cached;

            var pixelSize = _brushImage.PixelSize;
            var target = new RenderTargetBitmap(pixelSize);
            using (var ctx = target.CreateDrawingContext(true))
            {
                ctx.FillRectangle(new SolidColorBrush(color), new Rect(0, 0, pixelSize.Width, pixelSize.Height));
                var options = new RenderOptions { BitmapBlendingMode = BitmapBlendingMode.DestinationIn };
                using (ctx.PushRenderOptions(options))
                {
                    ctx.DrawImage(
                        _brushImage,
                        new Rect(0, 0, pixelSize.Width, pixelSize.Height),
                        new Rect(0, 0, pixelSize.Width, pixelSize.Height)
                    );
                }
            }
            _tintedCache[key] = target;
            if (_tintedCache.Count > 20)
            {
                var firstKey = _tintedCache.Keys.First();
                _tintedCache.Remove(firstKey);
            }
            return target;
        }

        private double GetStrokeSize(Stroke stroke)
        {
            return stroke.Size switch
            {
                SizeType.Small => 10,
                SizeType.Medium => 25,
                SizeType.Large => 60,
                SizeType.ExtraLarge => 150,
                _ => 10
            };
        }
    }
}