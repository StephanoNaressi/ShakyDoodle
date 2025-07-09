using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;

namespace ShakyDoodle.Utils
{
    public class ColorTools
    {
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
    }
}
