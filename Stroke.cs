
using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace ShakyDoodle
{
    public class Stroke
    {
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
        public List<Point> Points { get; } = new();
        public List<float> Pressures { get; } = new();
        public ColorType Color { get; }
        public SizeType Size { get; }
        public double Alpha {  get; }
        public PenLineCap PenLineCap { get; }
        public Stroke(ColorType color, Point startPoint, SizeType size, double alpha, PenLineCap cap, float startPressure)
        {
            Color = color;
            Points.Add(startPoint);
            Size = size;
            Alpha = alpha;
            PenLineCap = cap;
            Pressures.Add(startPressure);
        }
    }
}
