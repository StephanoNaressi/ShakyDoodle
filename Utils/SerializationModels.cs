using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using ShakyDoodle.Models;

namespace ShakyDoodle.Utils
{
    public class SerializableProject
    {
        public int CurrentFrame { get; set; }
        public int ActiveLayerIndex { get; set; }
        public List<SerializableFrame> Frames { get; set; } = new();
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
        public string Version { get; set; } = "1.0";
    }

    public class SerializableFrame
    {
        public List<SerializableLayer> Layers { get; set; } = new();
    }

    public class SerializableLayer
    {
        public double Opacity { get; set; } = 1.0;
        public string Name { get; set; } = "";
        public bool IsVisible { get; set; } = true;
        public List<SerializableStroke> Strokes { get; set; } = new();
    }

    public class SerializableStroke
    {
        public DateTime CreatedAt { get; set; }
        public List<SerializablePoint> Points { get; set; } = new();
        public List<float> Pressures { get; set; } = new();
        public SerializableColor Color { get; set; }
        public SizeType Size { get; set; }
        public double Alpha { get; set; }
        public PenLineCap PenLineCap { get; set; }
        public bool Shake { get; set; }
        public BrushType BrushType { get; set; }
    }

    public class SerializablePoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class SerializableColor
    {
        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
    }
}