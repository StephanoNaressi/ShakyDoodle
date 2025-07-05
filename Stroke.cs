
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace ShakyDoodle
{
    public class Stroke
    {
        public List<Point> Points { get; } = new();
        public ColorType Color { get; }
        public SizeType Size { get; }
        public double Alpha {  get; }
        public PenLineCap PenLineCap { get; }
        public Stroke(ColorType color, Point startPoint, SizeType size, double alpha, PenLineCap cap)
        {
            Color = color;
            Points.Add(startPoint);
            Size = size;
            Alpha = alpha;
            PenLineCap = cap;
        }
    }
}
