using System;
using Avalonia.Media;
using ShakyDoodle.Models;

namespace ShakyDoodle.Utils
{
    public class BrushHelper
    {
        private readonly ColorTools _colorHelper = new();
        
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

        public Pen ChooseBrushSettings(Stroke stroke, IBrush? overrideBrush, float rawPressure)
        {
            var size = GetStrokeSize(stroke);
            double thickness = size * rawPressure;
            if (overrideBrush != null)
            {
                return new Pen(overrideBrush, thickness)
                {
                    LineCap = stroke.PenLineCap
                };
            }
            var color = stroke.Color;
            return new Pen(new SolidColorBrush(color), thickness) {LineCap = stroke.PenLineCap};
        }

        public Pen ChooseBrushSettings(Stroke stroke, double scaledPressure, double rawPressure)
        {
            var color = stroke.Color;
            var size = GetStrokeSize(stroke);

            return new Pen(new SolidColorBrush(color, stroke.Alpha * scaledPressure), size * rawPressure);
        }
        
        public double GetStrokeSize(Stroke stroke)
        {
            return stroke.Size switch
            {
                SizeType.Small => 5,
                SizeType.Medium => 25,
                SizeType.Large => 60,
                SizeType.ExtraLarge => 150,
                _ => 5
            };
        }
        
        public IBrush GetSolidBrush(Color color, double alpha)
        {
            return new SolidColorBrush(color, alpha);
        }

        #endregion
    }
}
