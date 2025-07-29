using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Rendering.Composition;

namespace ShakyDoodle.Utils
{
    public class ColorTools
    {
        public static ColorTools Instance { get; } = new ColorTools();

        private readonly Random _random = new();
        
        public Color BackgroundColor { get; set; } = Colors.White;
        public Color GridColor { get; set; } = Colors.LightBlue;
        
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
                    int x = _random.Next(width);
                    int y = _random.Next(height);
                    byte red = (byte)_random.Next(0, 256);
                    byte green = (byte)_random.Next(0, 256);
                    byte blue = (byte)_random.Next(0, 256);

                    var brush = new SolidColorBrush(Color.FromArgb(alpha, red, green, blue));
                    ctx.FillRectangle(brush, new Rect(x, y, 1, 1));
                }
            }

            return renderTarget;
        }

        public void SetAvaloniaBGColor(BGColor bgColor)
        {
            var color = bgColor switch
            {
                BGColor.White => Colors.White,
                BGColor.Gray => Colors.LightGray,
                BGColor.DarkGray => Colors.DarkGray,
                BGColor.Yellow => Colors.LightYellow,
                BGColor.Black => Colors.Black,
                _ => Colors.White
            };
            BackgroundColor = color;
        }

        public void SetAvaloniaGridColor(BGColor bgColor)
        {
            var color = bgColor switch
            {
                BGColor.White => Colors.LightBlue,
                BGColor.Gray => Colors.DarkGray,
                BGColor.DarkGray => Colors.DimGray,
                BGColor.Yellow => Colors.SandyBrown,
                BGColor.Black => Colors.Gray,
                _ => Colors.White
            };
            GridColor = color;
        }
    }
}
