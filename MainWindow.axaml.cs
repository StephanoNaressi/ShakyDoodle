using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.Timers;
using Avalonia;
using Avalonia.Layout;
using ShakyDoodle.Views.Controls;
namespace ShakyDoodle
{
    public partial class MainWindow : Window
    {
        private readonly UIManager _uiManager = new UIManager();

        public MainWindow()
        {
            InitializeComponent();

            this.KeyDown += OnGlobalKeyDown;

            doodleCanvas.FrameController.OnFrameChanged = (current, total) =>
            {
                FrameIndicator.Text = $"Frames: {current}/{total}";
            };
            doodleCanvas.FrameController.OnLayerChanged = (current, total) =>
            {
                LayerIndicator.Text = $"Layers: {current}/{total}";
            };
            colorPicker.Color = Colors.Black;
            UpdateLayerLabel();
            UpdateFrameLabel();
            this.SizeChanged += (s, e) =>
            {
                doodleCanvas.InvalidateVisual();
            };
            this.PointerPressed += (s, e) =>
            {
                if (TooltipsPopup.IsOpen && !TooltipsPopup.Bounds.Contains(e.GetPosition(this)))
                {
                    TooltipsPopup.IsOpen = false;
                }
            };

            // Register button groups with the UIManager
            _uiManager.RegisterButtonGroup("brushes", new[] { unshakeButton, shakeButton, acrButton, airbrushButton, lassoFillButton, eraseButton });
            _uiManager.RegisterButtonGroup("sizes", new[] { sizeSmallButton, sizeMediumButton, sizeLargeButton, sizeXLargeButton });
            _uiManager.RegisterButtonGroup("tips", new[] { brushRoundButton, brushSquareButton, brushFlatButton });

            // Set initial selection
            _uiManager.UpdateSelection("brushes", unshakeButton);
            _uiManager.UpdateSelection("sizes", sizeSmallButton);
            _uiManager.UpdateSelection("tips", brushRoundButton);
        }

        private void OnGlobalKeyDown(object? sender, KeyEventArgs e)
        {
            var isCtrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            if (isCtrl && e.Key == Key.Z)
            {
                doodleCanvas.HandleUndo();
                e.Handled = true;
            }
            else if (isCtrl && e.Key == Key.Y)
            {
                doodleCanvas.HandleRedo();
                e.Handled = true;
            }
        }
        private void PopulateRecentColorSwatches()
        {
            RecentColorsPanel.Children.Clear();

            const int colorsPerRow = 6;
            int index = 0;
            StackPanel currentRow = null;

            foreach (var color in doodleCanvas.InputHandler.RecentColors)
            {
                if (index % colorsPerRow == 0)
                {
                    currentRow = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(0, 2, 0, 2),
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
                    };
                    RecentColorsPanel.Children.Add(currentRow);
                }

                var swatchColor = color; // needed to avoid closure issue
                var swatch = new Button
                {
                    Width = 16,
                    Height = 16,
                    Background = new SolidColorBrush(swatchColor),
                    Margin = new Thickness(2),
                    BorderBrush = new SolidColorBrush(Colors.Black),
                    BorderThickness = new Thickness(1),
                };

                swatch.Click += (s, e) =>
                {
                    ChangeColor(swatchColor);
                };

                currentRow.Children.Add(swatch);
                index++;
            }
        }



        private void UpdateFrameLabel() => FrameIndicator.Text = $"Frames: {doodleCanvas.FrameController.CurrentFrame + 1}/{doodleCanvas.FrameController.TotalFrames}";
        private void UpdateLayerLabel() => LayerIndicator.Text = $"Layers: {doodleCanvas.FrameController.ActiveLayerIndex + 1}/{doodleCanvas.FrameController.TotalLayers}";
        private void UpdatePlayLabel() => PlayButton.Content = PlayButton.Content as string == "▶" ? "■" : "▶";
        private void OnClearClick(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.ClearCanvas();
            UpdateFrameLabel();
            UpdateLayerLabel();
            layerOpacitySlider.Value = doodleCanvas.CurrentLayerOpacity() * 100;
        }
        private void OnDeleteFrame(object? sender, RoutedEventArgs events)
        {

            layerOpacitySlider.Value = doodleCanvas.CurrentLayerOpacity() * 100;
            doodleCanvas.DeleteCurrentFrame();
            UpdateFrameLabel();
            UpdateLayerLabel();
        }
        private void OnDeleteLayer(object? sender, RoutedEventArgs events)
        {
            layerOpacitySlider.Value = doodleCanvas.CurrentLayerOpacity() * 100;
            doodleCanvas.OnDeleteLayer();
            UpdateFrameLabel();
            UpdateLayerLabel();
        }
        private void OnRecenterClicked(object? sender, RoutedEventArgs events)
        {
            var zoomBorder = this.FindControl<ZoomBorder>("zoomBorder");
            zoomBorder?.Recenter();
        }

        private void UpdateLayerOpacity(object? sender, RoutedEventArgs events)
        {
            double newOpacity = layerOpacitySlider.Value / 100.0;
            doodleCanvas.OnUpdateOpacity(newOpacity);
        }
        private void SelectSize(SizeType size, Button sender)
        {
            doodleCanvas.SelectSize(size);
            logoCanvas.SelectSize(size);
            _uiManager.UpdateSelection("sizes", sender);
        }

        private void OnSizeSmall(object? sender, RoutedEventArgs e) => SelectSize(SizeType.Small, (Button)sender);
        private void OnSizeMedium(object? sender, RoutedEventArgs e) => SelectSize(SizeType.Medium, (Button)sender);
        private void OnSizeLarge(object? sender, RoutedEventArgs e) => SelectSize(SizeType.Large, (Button)sender);
        private void OnSizeXLarge(object? sender, RoutedEventArgs e) => SelectSize(SizeType.ExtraLarge, (Button)sender);

        private void ChangeBrushTip(PenLineCap tip, Button sender)
        {
            doodleCanvas.ChangeBrushTip(tip);
            logoCanvas.ChangeBrushTip(tip);
            _uiManager.UpdateSelection("tips", sender);
        }

        private void OnBrushSquare(object? sender, RoutedEventArgs e) => ChangeBrushTip(PenLineCap.Square, (Button)sender);
        private void OnBrushFlat(object? sender, RoutedEventArgs e) => ChangeBrushTip(PenLineCap.Flat, (Button)sender);
        private void OnBrushRound(object? sender, RoutedEventArgs e) => ChangeBrushTip(PenLineCap.Round, (Button)sender);

        private void ChangeBrushType(BrushType type, bool enableTips, Button sender)
        {
            doodleCanvas.ChangeBrushType(type);
            logoCanvas.ChangeBrushType(type);
            if (type == BrushType.Standard || type == BrushType.Shaking)
            {
                doodleCanvas.ShouldShake(type == BrushType.Shaking);
                logoCanvas.ShouldShake(type == BrushType.Shaking);
            }
            _uiManager.UpdateSelection("brushes", sender);
            _uiManager.UpdateButtonsState(new[] { brushRoundButton, brushSquareButton, brushFlatButton }, enableTips);
        }

        private void OnErase(object? sender, RoutedEventArgs e)
        {
            doodleCanvas.OnErase();
            _uiManager.UpdateSelection("brushes", (Button)sender);
            _uiManager.UpdateButtonsState(new[] { brushRoundButton, brushSquareButton, brushFlatButton }, false);
        }

        private void OnUnshake(object? sender, RoutedEventArgs e) => ChangeBrushType(BrushType.Standard, true, (Button)sender);
        private void OnShake(object? sender, RoutedEventArgs e) => ChangeBrushType(BrushType.Shaking, true, (Button)sender);
        private void OnAcr(object? sender, RoutedEventArgs e) => ChangeBrushType(BrushType.Acrylic, false, (Button)sender);
        private void OnAirbrush(object? sender, RoutedEventArgs e) => ChangeBrushType(BrushType.Airbrush, false, (Button)sender);
        private void OnLassoFill(object? sender, RoutedEventArgs e) => ChangeBrushType(BrushType.Lasso, false, (Button)sender);

        private void OnToggleNoise(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.ToggleNoise();
        }
        
        private void OnAlphaChanged(object? sender, RangeBaseValueChangedEventArgs events)
        {
            doodleCanvas.ChangeAlpha((double)events.NewValue);
            logoCanvas.ChangeAlpha((double)events.NewValue);
        }

        private void OnDuplicateFrame(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.DuplicateFrame();
            UpdateFrameLabel();
            UpdateLayerLabel();
        }

        private void OnNextFrame(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.NextFrame();
            UpdateFrameLabel();
            UpdateLayerLabel();

        }

        private void OnPrevFrame(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.PreviousFrame();
            UpdateFrameLabel();
            UpdateLayerLabel();

        }
        private void OnNextLayer(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.NextLayer();
            UpdateFrameLabel();
            UpdateLayerLabel();
            layerOpacitySlider.Value = doodleCanvas.CurrentLayerOpacity() * 100;
        }

        private void OnPrevLayer(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.PrevLayer();
            UpdateFrameLabel();
            UpdateLayerLabel();
            layerOpacitySlider.Value = doodleCanvas.CurrentLayerOpacity() * 100;
        }
        private void OnTogglePlay(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.TogglePlay();
            UpdatePlayLabel();
            UpdateLayerLabel();
        }
        
        private void ChangeColor(Color newColor)
        {
            doodleCanvas.ChangeColor(newColor);
            logoCanvas.ChangeColor(newColor);
            colorPicker.Color = newColor;
            PopulateRecentColorSwatches();
        }

        private void OnColorChanged(object? sender, ColorChangedEventArgs e) => ChangeColor(e.NewColor);
        private void OnChangeBlack(object? sender, RoutedEventArgs e) => ChangeColor(Colors.Black);
        private void OnChangeRed(object? sender, RoutedEventArgs e) => ChangeColor(Colors.Red);
        private void OnChangeBlue(object? sender, RoutedEventArgs e) => ChangeColor(Colors.Blue);
        private void OnChangeGreen(object? sender, RoutedEventArgs e) => ChangeColor(Colors.Green);

        private void ToggleLightbox(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.ToggleLightbox();
        }
        private void ToggleGrid(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.ToggleBackground(BGType.Grid);
        }
        private void ToggleLines(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.ToggleBackground(BGType.Lines);

        }
        private void ToggleBlank(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.ToggleBackground(BGType.Blank);
        }
        private void OnSaveFile(object? sender, RoutedEventArgs e)
        {
            string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            string folderName = "ShakyDoodle";
            string folderPath = Path.Combine(baseFolder, folderName);

            Directory.CreateDirectory(folderPath);

            int width = (int)doodleCanvas.Bounds.Width;
            int height = (int)doodleCanvas.Bounds.Height;

            doodleCanvas.ExportFramesAsPng(folderPath, width, height);
        }
        private void OnSaveGif(object? sender, RoutedEventArgs e)
        {
            string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            string folderName = "ShakyDoodle";
            string folderPath = Path.Combine(baseFolder, folderName);

            Directory.CreateDirectory(folderPath);

            // Add a filename to the path
            string fileName = $"animation_{DateTime.Now:yyyyMMdd_HHmmss}.gif";
            string filePath = Path.Combine(folderPath, fileName);

            int width = (int)doodleCanvas.Bounds.Width;
            int height = (int)doodleCanvas.Bounds.Height;

            doodleCanvas.ExportFramesAsGif(filePath, width, height);
        }
        
        private void OnTips(object? sender, RoutedEventArgs events)
        {
            TooltipsPopup.IsOpen = true;
        }
        private void OnTipsOff(object? sender, PointerPressedEventArgs e)
        {
            TooltipsPopup.IsOpen = false;
        }
    }
}