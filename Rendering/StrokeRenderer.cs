using Avalonia.Media;
using Avalonia;
using System.Collections.Generic;
using ShakyDoodle.Utils;
using ShakyDoodle.Models;
using System.Threading.Tasks;
using System.Linq;
using System;
using ShakyDoodle.Controllers;
using ShakyDoodle.Models;

namespace ShakyDoodle.Rendering
{
    public class StrokeRenderer
    {
        private ShakeController _shakeController = new();
        private BrushHelper _brushHelper = new();
        private AvaloniaExtras _helper;
        private int _gridSize = 50;
        private Pen _gridPen = new(Brushes.LightBlue, 1);
        int _maxStrokes = 150;

        #region Rendering

        public StrokeRenderer(Rect bounds, AvaloniaExtras helper)
        {
            _helper = helper;
        }
        public void Render(DrawingContext context, bool lightbox, int currentFrame, List<Stroke> strokes, List<Frame> frames, Rect bounds)
        {
            //PushPop to avoid drawing outside the canvas
            using var clip = context.PushClip(new Rect(bounds.Size));
            context.FillRectangle(Brushes.White, new Rect(bounds.Size));
            DrawGrid(context, bounds);

            if (lightbox)
            {
                DrawFrameIfExists(currentFrame - 1, Brushes.LightBlue, frames, context);
                DrawFrameIfExists(currentFrame + 1, Brushes.Pink, frames, context);
            }
            // Draw all existing strokes with shake effect based on their index
            for (int i = 0; i < strokes.Count; i++)
            {
                double shakeIntensity = _shakeController.GetShakeIntensity(i, strokes, _maxStrokes);
                DrawStroke(strokes[i], shakeIntensity, context);
            }
        }

        public void DrawGrid(DrawingContext context, Rect bounds)
        {
            for (int i = 0; i <= bounds.Width; i += _gridSize)
            {
                context.DrawLine(_gridPen, new Point(i, 0), new Point(i, bounds.Height));
            }
            for (int j = 0; j <= bounds.Height; j += _gridSize)
            {
                context.DrawLine(_gridPen, new Point(0, j), new Point(bounds.Width, j));
            }
        }
        private void DrawStroke(Stroke stroke, double shakeIntensity, DrawingContext context)
        {
            if (!stroke.Shake)
                shakeIntensity = 0;

            for (int i = 1; i < stroke.Points.Count; i++)
            {
                var pen = _brushHelper.SetupMainPen(stroke, i, shakeIntensity);
                var p1 = _shakeController.GetShakenPoint(stroke.Points[i - 1], shakeIntensity);
                var p2 = _shakeController.GetShakenPoint(stroke.Points[i], shakeIntensity);
                context.DrawLine(pen, p1, p2);
            }
        }
        public void DrawStrokeWithColorOverride(Stroke stroke, double shakeIntensity, IBrush overrideColor, DrawingContext context)
        {
            if (!stroke.Shake) shakeIntensity = 0;

            var pen = new Pen(overrideColor, _brushHelper.GetStrokeSize(stroke));
            pen.LineCap = stroke.PenLineCap;

            for (int i = 1; i < stroke.Points.Count; i++)
            {
                var p1 = _shakeController.GetShakenPoint(stroke.Points[i - 1], shakeIntensity);
                var p2 = _shakeController.GetShakenPoint(stroke.Points[i], shakeIntensity);
                context.DrawLine(pen, p1, p2);
            }
        }
        private void DrawFrameIfExists(int i, IBrush color, List<Frame> frames, DrawingContext context)
        {
            //If our index is invalid we dont do nothin'
            if (i < 0 || i >= frames.Count) return;
            //We grab the frame 
            var f = frames[i];
            //Loop through its strokes
            foreach (var stroke in f.Strokes)
            {
                DrawStrokeWithColorOverride(stroke, 0, color, context);
            }
        }

        public async void StartRenderLoopAsync(Func<List<Stroke>> getStrokes,Func<float> getSpeed)
        {
            while (true)
            {
                var strokes = getStrokes();
                bool anyShakeStrokes = strokes.Any(s => s.Shake);
                if (anyShakeStrokes)
                    _shakeController.UpdateTime(getSpeed());

                _helper.RequestInvalidateThrottled();
                await Task.Delay(16);
            }
        }

        #endregion

    }
}
