using Avalonia;
using Avalonia.Threading;

namespace ShakyDoodle.Utils
{
    public class AvaloniaExtras
    {
        private bool _renderPending;
        private readonly Visual _visual; 

        public AvaloniaExtras(Visual visual)
        {
            _visual = visual;
        }

        public void RequestInvalidateThrottled()
        {
            if (_renderPending)
                return;

            _renderPending = true;

            Dispatcher.UIThread.Post(() =>
            {
                _renderPending = false;
                _visual.InvalidateVisual();
            }, DispatcherPriority.Background);
        }
    }
}
