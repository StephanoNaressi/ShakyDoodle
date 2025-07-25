using System;
using Avalonia;

namespace ShakyDoodle.Utils
{
    public sealed class MathExtras
    {
        private MathExtras() { }

        private static readonly MathExtras _instance = new MathExtras();

        public static MathExtras Instance => _instance;

        public double Distance(Point p1, Point p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        public Point CatmullRom(Point p0, Point p1, Point p2, double t)
        {
            double t2 = t * t;
            double t3 = t2 * t;
            double x = 0.5 * ((2 * p1.X) +
                              (-p0.X + p2.X) * t +
                              (2 * p0.X - 5 * p1.X + 3 * p2.X) * t2 +
                              (-p0.X + 3 * p1.X - 2 * p2.X) * t3);
            double y = 0.5 * ((2 * p1.Y) +
                              (-p0.Y + p2.Y) * t +
                              (2 * p0.Y - 5 * p1.Y + 3 * p2.Y) * t2 +
                              (-p0.Y + 3 * p1.Y - 2 * p2.Y) * t3);
            return new Point(x, y);
        }
        public double Velocity(double dt, double dist) => dt > 0 ? dist / dt : 0;
    }
}
