using Avalonia.Media;
using ShakyDoodle.Controllers;
using System;

namespace ShakyDoodle.Models.Brushes
{
    public class ShakingBrush : IBrush
    {
        private readonly ShakeController _shakeController;

        public ShakingBrush(ShakeController shakeController)
        {
            _shakeController = shakeController;
        }

        public void DrawStroke(Stroke stroke, DrawingContext context, double layerOpacity = 1.0, double shakeIntensity = 1.0)
        {
            for (int i = 1; i < stroke.Points.Count; i++)
            {
                var pen = SetupPen(stroke, i, layerOpacity);
                var p1 = _shakeController.GetShakenPoint(stroke.Points[i - 1], shakeIntensity);
                var p2 = _shakeController.GetShakenPoint(stroke.Points[i], shakeIntensity);
                context.DrawLine(pen, p1, p2);
            }
        }

        private Pen SetupPen(Stroke stroke, int i, double layerOpacity)
        {
            float pressure = stroke.Pressures[i];
            double scaledPressure = Math.Min(pressure * 2, 1);
            var size = GetStrokeSize(stroke);
            
            return new Pen(
                new SolidColorBrush(stroke.Color, stroke.Alpha * scaledPressure * layerOpacity), 
                size * pressure)
            {
                LineCap = stroke.PenLineCap
            };
        }

        private double GetStrokeSize(Stroke stroke)
        {
            return stroke.Size switch
            {
                SizeType.Small => 5,
                SizeType.Medium => 25,
                SizeType.Large => 60,
                _ => 5
            };
        }
    }
}