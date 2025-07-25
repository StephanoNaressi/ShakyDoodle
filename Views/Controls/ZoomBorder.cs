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
            else if (e.Key == Key.D1) // Zoom out
            {
                ApplyZoom(1 / 1.1, new Point(Bounds.Width / 2, Bounds.Height / 2));
            }
            else if (e.Key == Key.D2) // Zoom in
            {
                ApplyZoom(1.1, new Point(Bounds.Width / 2, Bounds.Height / 2));
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

            var scaleFactor = e.Delta.Y > 0 ? 1.2 : 1 / 1.2;
            ApplyZoom(scaleFactor, e.GetPosition(_child));
        }

        private void ApplyZoom(double scaleFactor, Point center)
        {
            if (_child == null) return;

            var transformGroup = _child.RenderTransform as TransformGroup;
            if (transformGroup?.Children[0] is not ScaleTransform scaleTransform ||
                transformGroup.Children[1] is not TranslateTransform translateTransform)
            {
                return;
            }

            var currentScale = scaleTransform.ScaleX;
            var newScale = currentScale * scaleFactor;
            _zoom = newScale;

            scaleTransform.ScaleX = newScale;
            scaleTransform.ScaleY = newScale;

            var currentTranslation = new Point(translateTransform.X, translateTransform.Y);
            var newTranslationX = currentTranslation.X - (center.X * newScale - center.X * currentScale);
            var newTranslationY = currentTranslation.Y - (center.Y * newScale - center.Y * currentScale);

            translateTransform.X = newTranslationX;
            translateTransform.Y = newTranslationY;
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