using Avalonia;
using Avalonia.Media;
using ShakyDoodle.Models;

namespace ShakyDoodle.Utils.Brushes
{
    public interface IBrush
    {
        void DrawStroke(Stroke stroke, DrawingContext context, double layerOpacity = 1.0, double shakeIntensity = 0.0);

    }
}