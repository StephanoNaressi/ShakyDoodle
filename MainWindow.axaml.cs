using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using ShakyDoodle.Services;
using ShakyDoodle.Utils;
using ShakyDoodle.Views.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ShakyDoodle
{
    public partial class MainWindow : Window
    {
        #region Fields

        private readonly UIManager _uiManager = new UIManager();
        private bool _framesLocked = false;
        private bool _isEyeDropperActive = false;
        private FileService? _fileService;
        private string _templatesFolder = string.Empty;
        private Color _defaultMain = Colors.PeachPuff, _defaultSecondary = Colors.Chocolate;
        #endregion

        #region Constructor

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

            doodleCanvas.FrameController.OnLockStateChanged = (isLocked) =>
            {
                _framesLocked = isLocked;
                LockFramesButton.Content = isLocked ? "🔒" : "🔓";
                DuplicateFrameButton.IsEnabled = !isLocked;
                DeleteFrameButton.IsEnabled = !isLocked;
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

            _uiManager.RegisterButtonGroup("brushes", new[] { unshakeButton, shakeButton, acrButton, airbrushButton, lassoFillButton, eraseButton, ditherButton, eyedropperButton });
            _uiManager.RegisterButtonGroup("sizes", new[] { sizeSmallButton, sizeMediumButton, sizeLargeButton, sizeXLargeButton });
            _uiManager.RegisterButtonGroup("tips", new[] { brushRoundButton, brushSquareButton, brushFlatButton });

            _uiManager.UpdateSelection("brushes", unshakeButton);
            _uiManager.UpdateSelection("sizes", sizeSmallButton);
            _uiManager.UpdateSelection("tips", brushRoundButton);
            _uiManager.UpdateMenuSelection("backgrounds", backgroundWhiteButton);

            LockFramesButton.IsEnabled = true;

            _fileService = new FileService(doodleCanvas.FrameController);
            InitializeTemplates();
            ChangeUIColors(_defaultMain, _defaultSecondary);
        }

        #endregion
        #region Templates

        private void InitializeTemplates()
        {
            string executablePath = Assembly.GetExecutingAssembly().Location;
            string executableDirectory = Path.GetDirectoryName(executablePath) ?? string.Empty;

            _templatesFolder = Path.Combine(executableDirectory, "templates");

            if (!Directory.Exists(_templatesFolder))
            {
                Directory.CreateDirectory(_templatesFolder);
            }

            LoadTemplateMenuItems();
        }
        private void LoadTemplateMenuItems()
        {
            Templates.Items.Clear();

            if (!Directory.Exists(_templatesFolder))
                return;

            var shakyFiles = Directory.GetFiles(_templatesFolder, "*.shaky");

            if (shakyFiles.Length == 0)
            {
                var noTemplatesItem = new MenuItem
                {
                    Header = "No templates found",
                    IsEnabled = false
                };
                Templates.Items.Add(noTemplatesItem);
                return;
            }

            foreach (var filePath in shakyFiles.OrderBy(f => Path.GetFileNameWithoutExtension(f)))
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var menuItem = new MenuItem
                {
                    Header = fileName,
                    Tag = filePath
                };
                menuItem.Click += OnTemplateMenuItemClick;
                Templates.Items.Add(menuItem);
            }

            Templates.Items.Add(new Separator());
            var refreshItem = new MenuItem
            {
                Header = "Refresh Templates"
            };

            refreshItem.Click += OnRefreshTemplates;
            Templates.Items.Add(refreshItem);
            var folderOpen = new MenuItem
            {
                Header = "Open Template Folder"
            };

            folderOpen.Click += OnTemplateFolderOpen;
            Templates.Items.Add(folderOpen);
        }

        private void OnTemplateMenuItemClick(object? sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is string filePath)
            {
                LoadProjectFromPath(filePath);
            }
        }

        private void OnRefreshTemplates(object? sender, RoutedEventArgs e)
        {
            LoadTemplateMenuItems();
        }
        private void OnTemplateFolderOpen(object? sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{_templatesFolder}\"") { UseShellExecute = true });
        }
        #endregion
        #region Event Handlers - Global

        private void OnGlobalKeyDown(object? sender, KeyEventArgs e)
        {
            doodleCanvas.ShortcutHelper.HandleKeyDown(e);
            if (e.Handled)
            {
                UpdateFrameLabel();
                UpdateLayerLabel();
            }
        }

        #endregion

        #region Color Swatches

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

        #endregion

        #region UI Updates

        private void UpdateFrameLabel() => FrameIndicator.Text = $"Frames: {doodleCanvas.FrameController.CurrentFrame + 1}/{doodleCanvas.FrameController.TotalFrames}";
        private void UpdateLayerLabel() => LayerIndicator.Text = $"Layers: {doodleCanvas.FrameController.ActiveLayerIndex + 1}/{doodleCanvas.FrameController.TotalLayers}";
        private void UpdatePlayLabel() => PlayButton.Content = PlayButton.Content as string == "▶" ? "■" : "▶";
        private void OnLightMode(object? sender, RoutedEventArgs e) => ChangeUIColors(_defaultMain, _defaultSecondary);
        private void OnDarkMode(object? sender, RoutedEventArgs e) => ChangeUIColors(new Color(255, 28, 31, 43), new Color(255, 176, 179, 191));
        private void ChangeUIColors(Color main, Color secondary)
        {
            zoomBorder.Background = new SolidColorBrush(new Color(255,48,49,51));
            this.Background = new SolidColorBrush(main);
            notebookHoles.UpdateColors(main, secondary);

            // Update Menu
            MenuDock.Background = new SolidColorBrush(main);
            foreach (var item in MenuDock.Items.OfType<MenuItem>())
            {
                item.Foreground = new SolidColorBrush(secondary);
                UpdateMenuItemRecursively(item, secondary);
            }

            ToolsPanel.Background = new SolidColorBrush(main);

            UpdateTextBlocksRecursively(ToolsPanel, secondary);

            UpdateButtonsRecursively(ToolsPanel, secondary);

            if (this.FindControl<Border>("") is Border canvasBorder)
            {
                canvasBorder.BorderBrush = new SolidColorBrush(secondary);
            }

            var canvasAreaBorder = this.GetLogicalDescendants().OfType<Border>()
                .FirstOrDefault(b => b.BorderThickness.Left == 3);
            if (canvasAreaBorder != null)
            {
                canvasAreaBorder.BorderBrush = new SolidColorBrush(secondary);
            }

            if (TooltipsPopup?.Child is Border tooltipBorder)
            {
                tooltipBorder.BorderBrush = new SolidColorBrush(secondary);
                UpdateTextBlocksRecursively(tooltipBorder, secondary);
            }

            UpdateColorSwatchBorders(secondary);
        }

        private void UpdateMenuItemRecursively(MenuItem menuItem, Color textColor)
        {
            menuItem.Foreground = new SolidColorBrush(textColor);

            foreach (var subItem in menuItem.Items.OfType<MenuItem>())
            {
                UpdateMenuItemRecursively(subItem, textColor);
            }
        }

        private void UpdateTextBlocksRecursively(Control parent, Color textColor)
        {
            foreach (var child in parent.GetLogicalDescendants())
            {
                if (child is TextBlock textBlock)
                {
                    textBlock.Foreground = new SolidColorBrush(textColor);
                }
            }
        }

        private void UpdateButtonsRecursively(Control parent, Color borderAndTextColor)
        {
            foreach (var child in parent.GetLogicalDescendants())
            {
                if (child is Button button)
                {
                    button.Foreground = new SolidColorBrush(borderAndTextColor);
                    button.BorderBrush = new SolidColorBrush(borderAndTextColor);
                }
            }
        }

        private void UpdateColorSwatchBorders(Color borderColor)
        {
            foreach (var child in RecentColorsPanel.Children)
            {
                if (child is StackPanel row)
                {
                    foreach (var swatch in row.Children.OfType<Button>())
                    {
                        swatch.BorderBrush = new SolidColorBrush(borderColor);
                    }
                }
            }
        }
        #endregion

        #region Frame & Layer Actions

        private void OnClearClick(object? sender, RoutedEventArgs e)
        {
            doodleCanvas.ClearCanvas();
            UpdateFrameLabel();
            UpdateLayerLabel();
            layerOpacitySlider.Value = doodleCanvas.CurrentLayerOpacity() * 100;
        }

        private void OnDeleteFrame(object? sender, RoutedEventArgs e)
        {
            layerOpacitySlider.Value = doodleCanvas.CurrentLayerOpacity() * 100;
            doodleCanvas.DeleteCurrentFrame();
            UpdateFrameLabel();
            UpdateLayerLabel();
        }

        private void OnDeleteLayer(object? sender, RoutedEventArgs e)
        {
            layerOpacitySlider.Value = doodleCanvas.CurrentLayerOpacity() * 100;
            doodleCanvas.OnDeleteLayer();
            UpdateFrameLabel();
            UpdateLayerLabel();
        }

        private void OnRecenterClicked(object? sender, RoutedEventArgs e)
        {
            var zoomBorder = this.FindControl<ZoomBorder>("zoomBorder");
            zoomBorder?.Recenter();
        }

        private void UpdateLayerOpacity(object? sender, RoutedEventArgs e)
        {
            double newOpacity = layerOpacitySlider.Value / 100.0;
            doodleCanvas.OnUpdateOpacity(newOpacity);
        }

        #endregion

        #region Size & Tip Selection

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

        #endregion
        #region Symmetry
        private void OnToggleSymmetry(object? sender, RoutedEventArgs e) => doodleCanvas.ToggleSymmetry();
        #endregion
        #region Brush Type Selection

        private void ChangeBrushType(BrushType type, bool enableTips, Button sender)
        {
            _isEyeDropperActive = false;
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
        private void OnDither(object? sender, RoutedEventArgs e) => ChangeBrushType(BrushType.Dither, true, (Button)sender);
        private void OnShake(object? sender, RoutedEventArgs e) => ChangeBrushType(BrushType.Shaking, true, (Button)sender);
        private void OnAcr(object? sender, RoutedEventArgs e) => ChangeBrushType(BrushType.Acrylic, false, (Button)sender);
        private void OnAirbrush(object? sender, RoutedEventArgs e) => ChangeBrushType(BrushType.Airbrush, false, (Button)sender);
        private void OnLassoFill(object? sender, RoutedEventArgs e) => ChangeBrushType(BrushType.Lasso, false, (Button)sender);

        #endregion

        #region Miscellaneous Actions

        private void OnToggleNoise(object? sender, RoutedEventArgs e)
        {
            doodleCanvas.ToggleNoise();
        }

        private void OnAlphaChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            doodleCanvas.ChangeAlpha((double)e.NewValue);
            logoCanvas.ChangeAlpha((double)e.NewValue);
        }

        #endregion

        #region Frame & Layer Navigation

        private void OnDuplicateFrame(object? sender, RoutedEventArgs e)
        {
            doodleCanvas.DuplicateFrame();
            UpdateFrameLabel();
            UpdateLayerLabel();
        }

        private void OnNextFrame(object? sender, RoutedEventArgs e)
        {
            doodleCanvas.NextFrame();
            UpdateFrameLabel();
            UpdateLayerLabel();
        }

        private void OnPrevFrame(object? sender, RoutedEventArgs e)
        {
            doodleCanvas.PreviousFrame();
            UpdateFrameLabel();
            UpdateLayerLabel();
        }

        private void OnNextLayer(object? sender, RoutedEventArgs e)
        {
            doodleCanvas.NextLayer();
            UpdateFrameLabel();
            UpdateLayerLabel();
            layerOpacitySlider.Value = doodleCanvas.CurrentLayerOpacity() * 100;
        }

        private void OnPrevLayer(object? sender, RoutedEventArgs e)
        {
            doodleCanvas.PrevLayer();
            UpdateFrameLabel();
            UpdateLayerLabel();
            layerOpacitySlider.Value = doodleCanvas.CurrentLayerOpacity() * 100;
        }

        private void OnTogglePlay(object? sender, RoutedEventArgs e)
        {
            doodleCanvas.TogglePlay();
            UpdatePlayLabel();
            UpdateLayerLabel();
        }

        private void OnLockFramesClick(object? sender, RoutedEventArgs e)
        {
            doodleCanvas.FrameController.ToggleLock();
        }

        #endregion

        #region Color & Background

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

        private void ChangeBackgroundColor(BGColor bgColor)
        {
            doodleCanvas.SetBackgroundColor(bgColor);
        }

        private void OnBackgroundWhite(object? sender, RoutedEventArgs e) => ChangeBackgroundColor(BGColor.White);
        private void OnBackgroundGray(object? sender, RoutedEventArgs e) => ChangeBackgroundColor(BGColor.Gray);
        private void OnBackgroundDarkGray(object? sender, RoutedEventArgs e) => ChangeBackgroundColor(BGColor.DarkGray);
        private void OnBackgroundYellow(object? sender, RoutedEventArgs e) => ChangeBackgroundColor(BGColor.Yellow);
        private void OnBackgroundBlack(object? sender, RoutedEventArgs e) => ChangeBackgroundColor(BGColor.Black);

        #endregion

        #region Canvas Background Types

        private void ToggleLightbox(object? sender, RoutedEventArgs e)
        {
            doodleCanvas.ToggleLightbox();
        }
        private void ToggleGrid(object? sender, RoutedEventArgs e)
        {
            doodleCanvas.ToggleBackground(BGType.Grid);
        }
        private void ToggleLines(object? sender, RoutedEventArgs e)
        {
            doodleCanvas.ToggleBackground(BGType.Lines);
        }
        private void ToggleBlank(object? sender, RoutedEventArgs e)
        {
            doodleCanvas.ToggleBackground(BGType.Blank);
        }

        #endregion

        #region File Operations

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

            string fileName = $"animation_{DateTime.Now:yyyyMMdd_HHmmss}.gif";
            string filePath = Path.Combine(folderPath, fileName);

            int width = (int)doodleCanvas.Bounds.Width;
            int height = (int)doodleCanvas.Bounds.Height;

            doodleCanvas.ExportFramesAsGif(filePath, width, height);
        }
        private async void OnSaveProject(object? sender, RoutedEventArgs e)
        {
            var storageProvider = StorageProvider;
            if (storageProvider == null) return;

            var fileTypes = new FilePickerFileType[]
            {
                new("ShakyDoodle Files")
                {
                    Patterns = new[] { "*.shaky" }
                },
                FilePickerFileTypes.All
            };

            var options = new FilePickerSaveOptions
            {
                Title = "Save ShakyDoodle Project",
                DefaultExtension = "shaky",
                FileTypeChoices = fileTypes,
                SuggestedFileName = $"project_{DateTime.Now:yyyyMMdd_HHmmss}.shaky"
            };

            var result = await storageProvider.SaveFilePickerAsync(options);
            if (result != null)
            {
                _fileService?.SaveProject(result.Path.LocalPath);
            }
        }

        private async void OnLoadProject(object? sender, RoutedEventArgs e)
        {
            var storageProvider = StorageProvider;
            if (storageProvider == null) return;

            var fileTypes = new FilePickerFileType[]
            {
                new("ShakyDoodle Files")
                {
                    Patterns = new[] { "*.shaky" }
                },
                FilePickerFileTypes.All
            };

            var options = new FilePickerOpenOptions
            {
                Title = "Load ShakyDoodle Project",
                FileTypeFilter = fileTypes,
                AllowMultiple = false
            };

            var result = await storageProvider.OpenFilePickerAsync(options);
            if (result != null && result.Count > 0)
            {
                _fileService?.LoadProject(result[0].Path.LocalPath);
                UpdateFrameLabel();
                UpdateLayerLabel();
                layerOpacitySlider.Value = doodleCanvas.CurrentLayerOpacity() * 100;
                doodleCanvas.InvalidateVisual();
            }
        }
        public void LoadProjectFromPath(string filePath)
        {
            if (File.Exists(filePath))
            {
                _fileService?.LoadProject(filePath);
            }
        }
        #endregion

        #region Tooltips

        private void OnTips(object? sender, RoutedEventArgs e)
        {
            TooltipsPopup.IsOpen = true;
        }
        private void OnTipsOff(object? sender, PointerPressedEventArgs e)
        {
            TooltipsPopup.IsOpen = false;
        }

        #endregion

        #region Eye Dropper

        private void OnEyeDropperClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                _uiManager.UpdateSelection("brushes", button);
            }

            bool enableTips = false;
            _uiManager.UpdateButtonsState(new[] { brushRoundButton, brushSquareButton, brushFlatButton }, enableTips);

            _isEyeDropperActive = true;
            this.Cursor = new Cursor(StandardCursorType.Cross);
            this.PointerPressed += OnEyeDropperPointerPressed;
        }

        private void OnEyeDropperPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (!_isEyeDropperActive) return;
            var position = e.GetPosition(this);

            var renderTarget = new RenderTargetBitmap(new PixelSize((int)Bounds.Width, (int)Bounds.Height));
            renderTarget.Render(this);

            var pixelColor = GetPixelColor(renderTarget, position);

            ChangeColor(pixelColor);

            this.Cursor = new Cursor(StandardCursorType.Arrow);
            this.PointerPressed -= OnEyeDropperPointerPressed;
        }

        private Color GetPixelColor(RenderTargetBitmap bitmap, Point position)
        {
            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            using var image = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(memoryStream);

            int x = (int)position.X;
            int y = (int)position.Y;

            if (x < 0 || y < 0 || x >= image.Width || y >= image.Height)
                return Colors.Transparent;

            var pixel = image[x, y];
            ChangeBrushType(BrushType.Standard, false, unshakeButton);
            return Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B);
        }

        #endregion

        #region Aspect Ratio

        private void On45AR(object? sender, RoutedEventArgs e) => SetAspectRatio(1100, 1375);
        private void On11AR(object? sender, RoutedEventArgs e) => SetAspectRatio(1400, 1400);
        private void On169AR(object? sender, RoutedEventArgs e) => SetAspectRatio(1956, 1100);
        private void On916AR(object? sender, RoutedEventArgs e) => SetAspectRatio(1100, 1956);
        private void OnA4HorizontalAR(object? sender, RoutedEventArgs e) => SetAspectRatio(1956, 1555);
        private void OnA4VerticalAR(object? sender, RoutedEventArgs e) => SetAspectRatio(1555, 1956);
        private void SetAspectRatio(double width, double height) => doodleCanvas.SetAspectRatio(width, height);

        private void MenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
        }

        #endregion
    }
}