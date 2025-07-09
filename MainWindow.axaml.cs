using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ShakyDoodle
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            this.KeyDown += OnGlobalKeyDown;

            doodleCanvas.FrameController.OnFrameChanged = (current, total) =>
            {
                FrameIndicator.Text = $"{current}/{total}";
            };
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

        private void UpdateFrameLabel() => FrameIndicator.Text = $"{doodleCanvas.FrameController.CurrentFrame + 1}/{doodleCanvas.FrameController.TotalFrames}";
        private void UpdatePlayLabel() => PlayButton.Content = PlayButton.Content as string == "▶" ? "■" : "▶";
        private void OnClearClick(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.ClearCanvas();
            UpdateFrameLabel();

        }

        private void OnFirstColor(object? sender, RoutedEventArgs events) {
            doodleCanvas.SelectColor(ColorType.First);
            logoCanvas.SelectColor(ColorType.First);
        }
        private void OnSecondColor(object? sender, RoutedEventArgs events) {
            doodleCanvas.SelectColor(ColorType.Second);
            logoCanvas.SelectColor(ColorType.Second);
        }
        private void OnThirdColor(object? sender, RoutedEventArgs events) {
            doodleCanvas.SelectColor(ColorType.Third);
            logoCanvas.SelectColor(ColorType.Third);
        }
        private void OnFourthColor(object? sender, RoutedEventArgs events) {
            doodleCanvas.SelectColor(ColorType.Fourth);
            logoCanvas.SelectColor(ColorType.Fourth);
        }
        private void OnFifthColor(object? sender, RoutedEventArgs events) {
            doodleCanvas.SelectColor(ColorType.Fifth);
            logoCanvas.SelectColor(ColorType.Fifth);
        }
        private void OnSixthColor(object? sender, RoutedEventArgs events) {
            doodleCanvas.SelectColor(ColorType.Sixth);
            logoCanvas.SelectColor(ColorType.Sixth);
        }
        private void OnSeventhColor(object? sender, RoutedEventArgs events) {
            doodleCanvas.SelectColor(ColorType.Seventh);
            logoCanvas.SelectColor(ColorType.Seventh);
        }
        private void OnEighthColor(object? sender, RoutedEventArgs events) {
            doodleCanvas.SelectColor(ColorType.Eighth);
            logoCanvas.SelectColor(ColorType.Eighth);
        }
        private void OnNinthColor(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.SelectColor(ColorType.Nineth);
            logoCanvas.SelectColor(ColorType.Nineth);
        }
        private void OnTenthColor(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.SelectColor(ColorType.Tenth);
            logoCanvas.SelectColor(ColorType.Tenth);
        }
        private void OnEleventhColor(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.SelectColor(ColorType.Eleventh);
            logoCanvas.SelectColor(ColorType.Eleventh);
        }
        private void OnTwelvethColor(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.SelectColor(ColorType.Twelveth);
            logoCanvas.SelectColor(ColorType.Twelveth);
        }
        private void OnThirteenthColor(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.SelectColor(ColorType.Thirteenth);
            logoCanvas.SelectColor(ColorType.Thirteenth);
        }
        private void OnFourteenthColor(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.SelectColor(ColorType.Fourteenth);
            logoCanvas.SelectColor(ColorType.Fourteenth);
        }
        private void OnFifteenthColor(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.SelectColor(ColorType.Fifteenth);
            logoCanvas.SelectColor(ColorType.Fifteenth);
        }
        private void OnSixteenthColor(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.SelectColor(ColorType.Sixteenth);
            logoCanvas.SelectColor(ColorType.Sixteenth);
        }

        private void OnSizeSmall(object? sender, RoutedEventArgs events) {
            doodleCanvas.SelectSize(SizeType.Small);
            logoCanvas.SelectSize(SizeType.Small);
        }
        private void OnSizeMedium(object? sender, RoutedEventArgs events) {
            doodleCanvas.SelectSize(SizeType.Medium);
            logoCanvas.SelectSize(SizeType.Medium);
        }
        private void OnSizeLarge(object? sender, RoutedEventArgs events) {
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

        }

        private void OnPrevFrame(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.PreviousFrame();
            UpdateFrameLabel();

        }

        private void OnTogglePlay(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.TogglePlay();
            UpdatePlayLabel();
        }

        private void ToggleOnionSkin(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.ToggleOnionSkin(true);
        }

        private void UntoggleOnionSkin(object? sender, RoutedEventArgs events)
        {
            doodleCanvas.ToggleOnionSkin(false);
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

    }
}