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
        public Color GetAvaloniaColor(ColorType colorType, double alpha = 1.0)
        {
            byte a = (byte)(alpha * 255);
            Color baseColor = colorType switch
            {
                ColorType.First => Colors.Black,
                ColorType.Second => Colors.MediumBlue,
                ColorType.Third => Colors.Firebrick,
                ColorType.Fourth => Colors.White,
                ColorType.Fifth => Colors.Yellow,
                ColorType.Sixth => Colors.GreenYellow,
                ColorType.Seventh => Colors.Purple,
                ColorType.Eighth => Colors.Pink,
                ColorType.Nineth => Colors.PeachPuff,
                ColorType.Tenth => Colors.SaddleBrown,
                ColorType.Eleventh => Colors.Thistle,
                ColorType.Twelveth => Colors.Tomato,
                ColorType.Thirteenth => Colors.Turquoise,
                ColorType.Fourteenth => Colors.SeaGreen,
                ColorType.Fifteenth => Colors.DarkSalmon,
                ColorType.Sixteenth => Colors.PaleVioletRed,
                _ => Colors.Black
            };

            return Color.FromArgb(a, baseColor.R, baseColor.G, baseColor.B);
        }

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
