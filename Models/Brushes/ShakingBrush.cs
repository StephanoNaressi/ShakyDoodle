using Avalonia.Media;
using ShakyDoodle.Controllers;
using System;
using System.Linq;

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
            if (stroke.Points.Count < 2)
                return;

            var strokeAlpha = stroke.Alpha * layerOpacity;
            var strokeBrush = new SolidColorBrush(stroke.Color, strokeAlpha);
            var pen = new Pen(
                strokeBrush,
                GetStrokeSize(stroke),
                lineCap: stroke.PenLineCap,
                lineJoin: PenLineJoin.Round
            );

            var geometry = new StreamGeometry();
            using (var geometryContext = geometry.Open())
            {
                var shakenPoints = stroke.Points.Select(p => _shakeController.GetShakenPoint(p, shakeIntensity)).ToList();
                geometryContext.BeginFigure(shakenPoints[0], false);
                for (int i = 1; i < shakenPoints.Count; i++)
                {
                    geometryContext.LineTo(shakenPoints[i]);
                }
                geometryContext.EndFigure(false);
            }

            context.DrawGeometry(strokeBrush, pen, geometry);
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