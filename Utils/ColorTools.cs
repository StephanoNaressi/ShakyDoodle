using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace ShakyDoodle.Utils
{
    public class ColorTools
    {
        public static ColorTools Instance { get; } = new ColorTools();

        Random _r = new();

        public bool AreColorsSimilar(Color a, Color b, int tolerance = 25)
        {
            return Math.Abs(a.R - b.R) < tolerance &&
                   Math.Abs(a.G - b.G) < tolerance &&
                   Math.Abs(a.B - b.B) < tolerance &&
                   Math.Abs(a.A - b.A) < tolerance;
        }

        public RenderTargetBitmap GenerateNoiseTexture(int width, int height, byte alpha = 50, double density = 10)
        {
            var pixelSize = new PixelSize(width, height);
            var renderTarget = new RenderTargetBitmap(pixelSize);

            using (var ctx = renderTarget.CreateDrawingContext(false))
            {
                for (int i = 0; i < width * height / density; i++)
                {
                    int x = _r.Next(width);
                    int y = _r.Next(height);
                    byte red = (byte)_r.Next(0, 256);
                    byte green = (byte)_r.Next(0, 256);
                    byte blue = (byte)_r.Next(0, 256);

                    var brush = new SolidColorBrush(Color.FromArgb(alpha, red, green, blue));
                    ctx.FillRectangle(brush, new Rect(x, y, 1, 1));
                }
            }

            return renderTarget;
        }



    }
}
