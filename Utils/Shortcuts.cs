using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using ShakyDoodle.Controllers;
using ShakyDoodle.Models;

namespace ShakyDoodle.Utils
{
    public class ShortcutHelper
    {
        private readonly Stack<List<Stroke>> _undoStack = new();
        private readonly Stack<List<Stroke>> _redoStack = new();
        private readonly FrameController _frameController;

        public ShortcutHelper(FrameController frameController)
        {
            _frameController = frameController;
        }

        public void HandleKeyDown(KeyEventArgs e)
        {
            var isCtrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);

            if (isCtrl)
            {
                switch (e.Key)
                {
                    case Key.Z:
                        Undo();
                        e.Handled = true;
                        break;
                    case Key.Y:
                        Redo();
                        e.Handled = true;
                        break;
                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.X:
                    case Key.Left:
                        _frameController.PreviousFrame();
                        e.Handled = true;
                        break;
                    case Key.C:
                    case Key.Right:
                        _frameController.NextFrame();
                        e.Handled = true;
                        break;
                    case Key.Up:
                        _frameController.SetCurrentLayer(_frameController.ActiveLayerIndex + 1);
                        e.Handled = true;
                        break;
                    case Key.Down:
                        _frameController.SetCurrentLayer(_frameController.ActiveLayerIndex - 1);
                        e.Handled = true;
                        break;
                }
            }
        }

        public void PushUndoState(List<Stroke> strokes)
        {
            _undoStack.Push(strokes.Select(s => s.Clone()).ToList());
            _redoStack.Clear();
        }

        public void Undo()
        {
            if (_undoStack.Count == 0) return;

            _redoStack.Push(_frameController.GetStrokes().Select(s => s.Clone()).ToList());
            var strokes = _undoStack.Pop();

            _frameController.SetStrokes(strokes);
            _frameController.SyncStrokesToFrame();
            _frameController.MarkDirty();
        }

        public void Redo()
        {
            if (_redoStack.Count == 0) return;

            _undoStack.Push(_frameController.GetStrokes().Select(s => s.Clone()).ToList());
            var strokes = _redoStack.Pop();

            _frameController.SetStrokes(strokes);
            _frameController.SyncStrokesToFrame();
            _frameController.MarkDirty();
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}
