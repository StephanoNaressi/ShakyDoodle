using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace ShakyDoodle
{
    public class NotebookHoles : Control
    {
        #region params
        public int HoleCount = 7;
        public double HoleDiameter = 20;
        public double HoleSpacing = 80;
        public Color HoleColor = Colors.White;
        public Color HoleBorderColor = Colors.Chocolate;
        public double HoleBorderThickness = 3;

        public int BindCount = 7;
        public double BindDiameter = 20;
        public double BindSpacing = 80;
        public Color BindColor = Colors.Gray;
        public Color BindBorderColor = Colors.Chocolate;
        public double BindBorderThickness = 3;
        #endregion
        public override void Render(DrawingContext context)
        {
            base.Render(context);

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
    }

}
