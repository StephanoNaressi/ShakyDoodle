using Avalonia;
using Avalonia.Media;
using ShakyDoodle.Models;

namespace ShakyDoodle.Models.Brushes
{
    public class StandardBrush : IBrush
    {
        public void DrawStroke(Stroke stroke, DrawingContext context, double layerOpacity = 1.0, double shakeIntensity = 0.0)
        {
            if (stroke.Points.Count < 2)
                return;

            // Apply both stroke.Alpha and layerOpacity
            var strokeBrush = new SolidColorBrush(stroke.Color, stroke.Alpha * layerOpacity);
            var pen = new Pen(
                strokeBrush,
                stroke.Size switch { SizeType.Small => 5, SizeType.Medium => 25, SizeType.Large => 60, SizeType.ExtraLarge => 150, _ => 5 },
                lineCap: stroke.PenLineCap,
                lineJoin: PenLineJoin.Round
            );

            var geometry = new StreamGeometry();
            using (var geometryContext = geometry.Open())
            {
                geometryContext.BeginFigure(stroke.Points[0], false);
                for (int i = 1; i < stroke.Points.Count; i++)
                {
                    geometryContext.LineTo(stroke.Points[i]);
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
                SizeType.ExtraLarge => 150,  
                _ => 5
            };
        }
    }
}