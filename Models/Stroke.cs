using Avalonia.Media;
using System.Collections.Generic;
using System;
using Avalonia;
namespace ShakyDoodle.Models
{
    public class Stroke
    {
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
        public List<Point> Points { get; } = new();
        public List<float> Pressures { get; } = new();
        public Color Color { get; }
        public SizeType Size { get; }
        public double Alpha { get; }
        public PenLineCap PenLineCap { get; }
        public bool Shake { get; }
        public BrushType BrushType { get; }
        public Stroke(Color color, Point startPoint, SizeType size, double alpha, PenLineCap cap, float startPressure, bool shake, BrushType brushType = BrushType.Standard)
        {
            Color = color;
            Points.Add(startPoint);
            Size = size;
            Alpha = alpha;
            PenLineCap = cap;
            Pressures.Add(startPressure);
            Shake = shake;
            BrushType = brushType;
        }

        public Stroke(Color color, List<Point> points, SizeType size, double alpha, PenLineCap cap, List<float> pressures, bool shake, BrushType brushType = BrushType.Standard)
        {
            Color = color;
            Points = new List<Point>(points);
            Size = size;
            Alpha = alpha;
            PenLineCap = cap;
            Pressures = new List<float>(pressures);
            Shake = shake;
            BrushType = brushType;
        }

        public Stroke Clone()
        {
            return new Stroke(Color, Points, Size, Alpha, PenLineCap, Pressures, Shake, BrushType);
        }
    }

}
