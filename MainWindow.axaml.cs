using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.Diagnostics;
using System.Timers;
using Avalonia.Threading;
using Avalonia;
using Avalonia.Layout;
namespace ShakyDoodle
{
    public partial class MainWindow : Window
    {
        private Timer _fpsTimer;
        private int _frameCount = 0;
        public MainWindow()
        {
            InitializeComponent();

            var frameSimTimer = new Timer(16); // ~60Hz
            frameSimTimer.Elapsed += (s, e) => _frameCount++;
            frameSimTimer.Start();

            _fpsTimer = new Timer(1000);
            _fpsTimer.Elapsed += FpsTimer_Elapsed;
            _fpsTimer.Start();

            this.KeyDown += OnGlobalKeyDown;

            doodleCanvas.FrameController.OnFrameChanged = (current, total) =>
            {
                FrameIndicator.Text = $"Frames: {current}/{total}";
            };
            doodleCanvas.FrameController.OnLayerChanged = (current, total) =>
            {
                LayerIndicator.Text = $"Layers: {current}/{total}";
            };
            colorPicker.Color = Avalonia.Media.Colors.Black;
            UpdateLayerLabel();
            UpdateFrameLabel();
            this.SizeChanged += (s, e) =>
            {
                doodleCanvas.InvalidateVisual();
            };
        }

        private void FpsTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            int fps = _frameCount;
            _frameCount = 0;

            Dispatcher.UIThread.Post(() =>
            {
                FpsCounter.Text = $"FPS: {fps}";
            });
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
                    doodleCanvas.ChangeColor(swatchColor);
                    colorPicker.Color = swatchColor;
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
        private void UpdateLayerOpacity(object? sender, RoutedEventArgs events)
        {
            double newOpacity = layerOpacitySlider.Value / 100.0;
            doodleCanvas.OnUpdateOpacity(newOpacity);
        }
        private void OnSizeSmall(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.SelectSize(SizeType.Small);
            logoCanvas.SelectSize(SizeType.Small);
        }
        private void OnErase(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.OnErase();
        }
        private void OnToggleNoise(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.ToggleNoise();
        }
        private void OnSizeMedium(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.SelectSize(SizeType.Medium);
            logoCanvas.SelectSize(SizeType.Medium);
        }
        private void OnSizeLarge(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.SelectSize(SizeType.Large);
            logoCanvas.SelectSize(SizeType.Large);
        }
        private void OnAlphaChanged(object? sender, RangeBaseValueChangedEventArgs events)
        {
            doodleCanvas.ChangeAlpha((double)events.NewValue);
            logoCanvas.ChangeAlpha((double)events.NewValue);
        }

        private void OnBrushSquare(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.ChangeBrushTip(Avalonia.Media.PenLineCap.Square);
            logoCanvas.ChangeBrushTip(Avalonia.Media.PenLineCap.Square);
        }

        private void OnBrushFlat(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.ChangeBrushTip(Avalonia.Media.PenLineCap.Flat);
            logoCanvas.ChangeBrushTip(Avalonia.Media.PenLineCap.Flat);
        }

        private void OnBrushRound(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.ChangeBrushTip(Avalonia.Media.PenLineCap.Round);
            logoCanvas.ChangeBrushTip(Avalonia.Media.PenLineCap.Round);
        }

        private void OnDuplicateFrame(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.DuplicateFrame();
            UpdateFrameLabel();
            UpdateLayerLabel();
        }

        private void OnShake(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.ShouldShake(true);
            logoCanvas.ShouldShake(true);
        }

        private void OnUnshake(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.ShouldShake(false);
            logoCanvas.ShouldShake(false);
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
        private void OnColorChanged(object? sender, ColorChangedEventArgs events)
        {
            var newColor = events.NewColor;
            doodleCanvas.ChangeColor(newColor);
            PopulateRecentColorSwatches();
        }
        private void OnChangeBlack(object? sender, RoutedEventArgs events)
        {
            var col = new Color(255, 0, 0, 0);
            doodleCanvas.ChangeColor(col);
            colorPicker.Color = col;
        }

        private void OnChangeRed(object? sender, RoutedEventArgs events)
        {
            var col = new Color(255, 255, 0, 0);
            doodleCanvas.ChangeColor(col);
            colorPicker.Color = col;
        }

        private void OnChangeBlue(object? sender, RoutedEventArgs events)
        {
            var col = new Color(255, 0, 0, 255);
            doodleCanvas.ChangeColor(col);
            colorPicker.Color = col;
        }

        private void OnChangeGreen(object? sender, RoutedEventArgs events)
        {
            var col = new Color(255, 0, 255, 0);
            doodleCanvas.ChangeColor(col);
            colorPicker.Color = col;
        }

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
    }
}