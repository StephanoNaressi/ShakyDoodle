using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace ShakyDoodle.Models
{
    public class Layer
    {
        public double Opacity = 1.0;
        public string Name;
        public bool IsVisible = true;
        public List<Stroke> Strokes = new();
        public RenderTargetBitmap? CachedBitmap { get; set; }
        public bool IsDirty { get; set; } = true;

    }
}
