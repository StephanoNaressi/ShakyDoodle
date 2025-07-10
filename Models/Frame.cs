using System.Collections.Generic;
using Avalonia.Media.Imaging;

namespace ShakyDoodle.Models
{
    public class Frame
    {
        public bool IsDirty { get; set; } = true;
        public RenderTargetBitmap? CachedBitmap { get; set; }
        public List<Layer> Layers { get; set; } = new();
    }
}
