using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using ShakyDoodle.Controllers;

namespace ShakyDoodle.Views.Controls
{
    public class ZoomBorder : Border
    {
        private Point _origin;
        private Point _start;
        private Control? _child;
        private double _zoom = 0.7;
        private DoodleCanvas? DoodleChild => this.Child as DoodleCanvas;

        public ZoomBorder()
        {
            this.PointerWheelChanged += OnPointerWheelChanged;
            this.PointerPressed += OnPointerPressed;
            this.PointerReleased += OnPointerReleased;
            this.PointerMoved += OnPointerMoved;
            this.KeyDown += OnKeyDown;
            this.KeyUp += OnKeyUp;
            this.Focusable = true;

            this.AttachedToVisualTree += (s, e) =>
            {
                _child = this.Child;
                if (_child != null)
                {
                    _child.RenderTransform = new TransformGroup
                    {
                        Children = new Transforms
                        {
                            new ScaleTransform { ScaleX = _zoom, ScaleY = _zoom },
                            new TranslateTransform()
                        }
                    };
                }
            };

        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && DoodleChild != null)
            {
                DoodleChild.IsSpacePressed = true;
            }
        }

        private void OnKeyUp(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && DoodleChild != null)
            {
                DoodleChild.IsSpacePressed = false;
                DoodleChild.IsPanning = false;
                DoodleChild.InputHandler.CancelStroke();
            }
        }

        private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            if (_child == null) return;

            var point = e.GetPosition(_child);
            var scale = e.Delta.Y > 0 ? 1.2 : 0.8;
            _zoom *= scale;

            var transformGroup = _child.RenderTransform as TransformGroup;
            var scaleTransform = transformGroup?.Children[0] as ScaleTransform;
            var translateTransform = transformGroup?.Children[1] as TranslateTransform;

            if (scaleTransform != null && translateTransform != null)
            {
                scaleTransform.ScaleX *= scale;
                scaleTransform.ScaleY *= scale;

                translateTransform.X = point.X - (point.X * scale);
                translateTransform.Y = point.Y - (point.Y * scale);
            }
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if ((DoodleChild?.IsSpacePressed ?? false) || e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed)
            {
                if (DoodleChild != null)
                    DoodleChild.IsPanning = true;
                _start = e.GetPosition(this);
                _origin = new Point(
                    (_child?.RenderTransform as TransformGroup)?.Children[1] is TranslateTransform t ? t.X : 0,
                    (_child?.RenderTransform as TransformGroup)?.Children[1] is TranslateTransform t2 ? t2.Y : 0);
            }
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (!(DoodleChild?.IsSpacePressed ?? false))
            {
                if (DoodleChild != null)
                {
                    DoodleChild.IsPanning = false;
                    DoodleChild.InputHandler.CancelStroke();
                }
            }
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (!(DoodleChild?.IsPanning ?? false)) return;

            var position = e.GetPosition(this);
            var delta = position - _start;

            if (_child?.RenderTransform is TransformGroup transformGroup)
            {
                if (transformGroup.Children[1] is TranslateTransform translateTransform)
                {
                    translateTransform.X = _origin.X + delta.X;
                    translateTransform.Y = _origin.Y + delta.Y;
                }
            }
        }
        public void Recenter()
        {
            _zoom = 0.7;
            if (_child?.RenderTransform is TransformGroup group)
            {
                if (group.Children[0] is ScaleTransform scale)
                {
                    scale.ScaleX = _zoom;
                    scale.ScaleY = _zoom;
                }
                if (group.Children[1] is TranslateTransform translate)
                {
                    translate.X = 0;
                    translate.Y = 0;
                }
                _child.InvalidateVisual();
            }
        }


    }
}