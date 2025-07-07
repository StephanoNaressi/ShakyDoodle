using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace ShakyDoodle
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnClearClick(object? sender, RoutedEventArgs events) => doodleCanvas.ClearCanvas();
        private void OnFirstColor(object? sender, RoutedEventArgs events) => doodleCanvas.SelectColor(ColorType.First);
        private void OnSecondColor(object? sender, RoutedEventArgs events) => doodleCanvas.SelectColor(ColorType.Second);
        private void OnThirdColor(object? sender, RoutedEventArgs events) => doodleCanvas.SelectColor(ColorType.Third);
        private void OnFourthColor(object? sender, RoutedEventArgs events) => doodleCanvas.SelectColor(ColorType.Fourth);
        private void OnFifthColor(object? sender, RoutedEventArgs events) => doodleCanvas.SelectColor(ColorType.Fifth);
        private void OnSixthColor(object? sender, RoutedEventArgs events) => doodleCanvas.SelectColor(ColorType.Sixth);
        private void OnSeventhColor(object? sender, RoutedEventArgs events) => doodleCanvas.SelectColor(ColorType.Seventh);
        private void OnEighthColor(object? sender, RoutedEventArgs events) => doodleCanvas.SelectColor(ColorType.Eighth);
        private void OnSizeSmall(object? sender, RoutedEventArgs events) => doodleCanvas.SelectSize(SizeType.Small);
        private void OnSizeMedium(object? sender, RoutedEventArgs events) => doodleCanvas.SelectSize(SizeType.Medium);
        private void OnSizeLarge(object? sender, RoutedEventArgs events) => doodleCanvas.SelectSize(SizeType.Large);
        private void OnAlphaChanged(object? sender, RangeBaseValueChangedEventArgs events) => doodleCanvas.ChangeAlpha((double)events.NewValue);
        private void OnBrushSquare(object? sender, RoutedEventArgs events) => doodleCanvas.ChangeBrushTip(Avalonia.Media.PenLineCap.Square);
        private void OnBrushFlat(object? sender, RoutedEventArgs events) => doodleCanvas.ChangeBrushTip(Avalonia.Media.PenLineCap.Flat);
        private void OnBrushRound(object? sender, RoutedEventArgs events) => doodleCanvas.ChangeBrushTip(Avalonia.Media.PenLineCap.Round);

        private void OnShake(object? sender, RoutedEventArgs events) => doodleCanvas.ShouldShake(true);
        private void OnUnshake(object? sender, RoutedEventArgs events) => doodleCanvas.ShouldShake(false);
        private void OnNextFrame(object? sender, RoutedEventArgs events) => doodleCanvas.NextFrame();
        private void OnPrevFrame(object? sender, RoutedEventArgs events) => doodleCanvas.PreviousFrame();
        private void OnTogglePlay(object? sender, RoutedEventArgs events) => doodleCanvas.TogglePlay();
        private void ToggleOnionSkin(object? sender, RoutedEventArgs events) => doodleCanvas.ToggleOnionSkin(true);
        private void UntoggleOnionSkin(object? sender, RoutedEventArgs events) => doodleCanvas.ToggleOnionSkin(false);


    }
}