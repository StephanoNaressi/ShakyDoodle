using System.Collections.Generic;
using Avalonia.Media.Imaging;

namespace ShakyDoodle.Models
{
    public class Layer
    {
        public double Opacity { get; set; } = 1.0;
        public string Name { get; set; } = string.Empty;
        public bool IsVisible { get; set; } = true;
        public List<Stroke> Strokes { get; set; } = new();
        public RenderTargetBitmap? CachedBitmap { get; set; }
        public bool IsDirty { get; set; } = true;
    }
}
