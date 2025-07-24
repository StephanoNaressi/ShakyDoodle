using Avalonia;
using Avalonia.Media;
using ShakyDoodle.Models;

namespace ShakyDoodle.Utils.Brushes
{
    public class StandardBrush : IBrush
    {
        public void DrawStroke(Stroke stroke, DrawingContext context, double layerOpacity = 1.0, double shakeIntensity = 0.0)
        {
            var strokeAlpha = stroke.Alpha * layerOpacity;
            var strokeBrush = new SolidColorBrush(stroke.Color, strokeAlpha);

            for (int i = 1; i < stroke.Points.Count; i++)
            {
                var pt = stroke.Points[i - 1];
                float pressure = i < stroke.Pressures.Count ? stroke.Pressures[i] : 1f;
                double size = GetStrokeSize(stroke) * pressure;

                switch (stroke.PenLineCap)
                {
                    case PenLineCap.Round:
                        context.DrawEllipse(strokeBrush, null, pt, size / 2, size / 2);
                        break;
                    case PenLineCap.Square:
                        context.DrawRectangle(strokeBrush, null,
                            new Rect(pt.X - size / 2, pt.Y - size / 2, size, size));
                        break;
                    case PenLineCap.Flat:
                        context.DrawRectangle(strokeBrush, null,
                            new Rect(pt.X - size / 2, pt.Y - size / 2, size / 2, size));
                        break;
                    default:
                        context.DrawEllipse(strokeBrush, null, pt, size / 2, size / 2);
                        break;
                }
            }
        }

        private double GetStrokeSize(Stroke stroke)
        {
            return stroke.Size switch
            {
                SizeType.Small => 2,
                SizeType.Medium => 8,
                SizeType.Large => 20,
                _ => 5
            };
        }
    }
}