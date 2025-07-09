using System;
using System.Collections.Generic;
using Avalonia;

namespace ShakyDoodle.Controllers
{
    public class ShakeController
    {
        private double _time;
        private float _amp = 2f;
        private float _speed = 0.3f;

        public double GetShakeIntensity(int strokeIndex, List<Stroke> strokes, int maxStrokes)
        {
            int count = strokes.Count;
            int newer = count - 1 - strokeIndex;
            double t = newer / (double)maxStrokes;
            return Math.Clamp(1.0 - t, 0.0, 1.0);
        }

        public Point GetShakenPoint(Point point, double shakeIntensity)
        {
            if (shakeIntensity <= 0) return point;

            double seedX = Math.Sin(point.X * 12.9898 + point.Y * 78.233);
            double seedY = Math.Cos(point.X * 93.9898 + point.Y * 67.345);

            double offsetX = seedX * 43758.5453 % 2 - 1;
            double offsetY = seedY * 12737.2349 % 2 - 1;

            double shakenX = point.X + Math.Sin(_time + offsetX * 10) * _amp * shakeIntensity;
            double shakenY = point.Y + Math.Cos(_time + offsetY * 10) * _amp * shakeIntensity;

            return new Point(shakenX, shakenY);
        }
        public double GetShakeTime() => _time;
        public double GetShakeAmp() => _amp;
        public float GetSpeed() => _speed;
        public void UpdateTime(double val) => _time += val;
    }
}
