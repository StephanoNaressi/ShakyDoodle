using System;
using Avalonia.Media;
using ShakyDoodle.Models;

namespace ShakyDoodle.Utils
{
    public class BrushHelper
    {
        private ColorTools _colorHelper = new();
        #region Brush Setup

        public Pen SetupMainPen(Stroke stroke, int i, double shakeIntensity)
        {
            Pen newPen = new();
            float pressure = stroke.Pressures[i];
            double scaledPressure = Math.Min(pressure * 2, 1);
            var brush = ChooseBrushSettings(stroke, scaledPressure, pressure);
            newPen = brush;
            newPen.LineCap = stroke.PenLineCap;
            return newPen;
        }

        public Pen ChooseBrushSettings(Stroke stroke, IBrush overrideColor, float rawPressure)
        {
            var size = GetStrokeSize(stroke);

            double scaledPressure = Math.Min(rawPressure * 2, 1);
            SolidColorBrush targetBrush;

            if (overrideColor is SolidColorBrush colorBrush)
            {
                targetBrush = new SolidColorBrush(colorBrush.Color, stroke.Alpha * scaledPressure);
            }
            else throw new NotImplementedException("Impossible to extract brush type");

            return new Pen(targetBrush, size * rawPressure);
        }

        public Pen ChooseBrushSettings(Stroke stroke, double scaledPressure, double rawPressure)
        {
            var color = _colorHelper.GetAvaloniaColor(stroke.Color);
            var size = GetStrokeSize(stroke);

            return new Pen(new SolidColorBrush(color, stroke.Alpha * scaledPressure), size * rawPressure);
        }
        public double GetStrokeSize(Stroke stroke)
        {
            return stroke.Size switch
            {
                SizeType.Small => 2,
                SizeType.Medium => 8,
                SizeType.Large => 20,
                _ => 5
            };
        }
        public IBrush GetSolidBrush(ColorType colorType, double alpha)
        {
            var color = _colorHelper.GetAvaloniaColor(colorType, alpha);
            return new SolidColorBrush(color);
        }

        #endregion
    }
}
