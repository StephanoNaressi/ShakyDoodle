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
        public ColorType Color { get; }
        public SizeType Size { get; }
        public double Alpha { get; }
        public PenLineCap PenLineCap { get; }
        public bool Shake { get; }

        public Stroke(ColorType color, Point startPoint, SizeType size, double alpha, PenLineCap cap, float startPressure, bool shake)
        {
            Color = color;
            Points.Add(startPoint);
            Size = size;
            Alpha = alpha;
            PenLineCap = cap;
            Pressures.Add(startPressure);
            Shake = shake;
        }

        public Stroke(ColorType color, List<Point> points, SizeType size, double alpha, PenLineCap cap, List<float> pressures, bool shake)
        {
            Color = color;
            Points = new List<Point>(points);
            Size = size;
            Alpha = alpha;
            PenLineCap = cap;
            Pressures = new List<float>(pressures);
            Shake = shake;
        }

        public Stroke Clone()
        {
            return new Stroke(Color, Points, Size, Alpha, PenLineCap, Pressures, Shake);
        }
    }

}
