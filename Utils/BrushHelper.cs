using System;
using Avalonia.Media;

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
        #endregion
    }
}
