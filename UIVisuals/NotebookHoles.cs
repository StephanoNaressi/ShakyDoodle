using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace ShakyDoodle.UIVisuals
{
    public class NotebookHoles : Control
    {
        #region Properties
        public int HoleCount { get; set; } = 7;
        public double HoleDiameter { get; set; } = 20;
        public double HoleSpacing { get; set; } = 80;
        public Color HoleColor { get; set; } = Colors.White;
        public Color HoleBorderColor { get; set; } = Colors.Chocolate;
        public double HoleBorderThickness { get; set; } = 3;

        public int BindCount { get; set; } = 7;
        public double BindDiameter { get; set; } = 20;
        public double BindSpacing { get; set; } = 80;
        public Color BindColor { get; set; } = Colors.Gray;
        public Color BindBorderColor { get; set; } = Colors.Chocolate;
        public double BindBorderThickness { get; set; } = 3;
        #endregion

        #region Private Fields
        private Color _backgroundColor = Colors.White;
        #endregion

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            context.FillRectangle(new SolidColorBrush(_backgroundColor), new Rect(0, 0, Bounds.Width, Bounds.Height));
            var brush = new SolidColorBrush(HoleColor);
            var pen = new Pen(new SolidColorBrush(HoleBorderColor), HoleBorderThickness);

            var bindBrush = new SolidColorBrush(BindColor);
            var bindPen = new Pen(new SolidColorBrush(BindBorderColor), BindBorderThickness);

            double x = HoleDiameter / 2 + 5;

            for (int i = 0; i < HoleCount; i++)
            {
                double y = 60 + i * HoleSpacing;
                var center = new Point(x, y);
                context.DrawEllipse(brush, pen, center, HoleDiameter / 2, HoleDiameter / 2);

                var offsetCenter = center - new Point(10, 0);
                var rect = new Rect(
                    offsetCenter.X - BindDiameter / 2,
                    offsetCenter.Y - (BindDiameter / 2 - 5),
                    BindDiameter,
                    (BindDiameter / 2 - 5) * 2
                );

                context.DrawRectangle(bindBrush, bindPen, rect);
            }
        }

        public void UpdateColors(Color main, Color secondary)
        {
            _backgroundColor = main;
            HoleBorderColor = secondary;
            BindBorderColor = secondary;
            InvalidateVisual();
        }
    }
}
