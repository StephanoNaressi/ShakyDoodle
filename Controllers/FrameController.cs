using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ShakyDoodle.Models;
using ShakyDoodle.Rendering;
using ShakyDoodle.Utils;

namespace ShakyDoodle.Controllers
{
    public class FrameController
    {
        StrokeRenderer _strokeRenderer;
        AvaloniaExtras _helper;
        public int CurrentFrame;
        public int TotalFrames => _frames.Count;

        private List<Frame> _frames = new();
        private List<Stroke> _strokes = new();

        public Action<int, int>? OnFrameChanged;

        private bool _isPlaying = false;
        public FrameController(Rect bounds, StrokeRenderer strokeRenderer, int startingFrame = 0)
        {
            _strokeRenderer = strokeRenderer;
            CurrentFrame = startingFrame;
        }

        public List<Stroke> GetStrokes() => _strokes;

        public void SetStrokes(List<Stroke> strokes)
        {
            _strokes = strokes.Select(s => s.Clone()).ToList();
        }

        public void SaveCurrentFrame()
        {
            var strokesCopy = _strokes.Select(s => s.Clone()).ToList();

            if (CurrentFrame < _frames.Count)
                _frames[CurrentFrame].Strokes = strokesCopy;
            else
                _frames.Add(new Frame { Strokes = strokesCopy });
        }

        public void LoadFrame(int index, Visual visual)
        {
            if (index < 0 || index >= _frames.Count) return;

            CurrentFrame = index;
            SyncFrameToStrokes();
            visual.InvalidateVisual();

            OnFrameChanged?.Invoke(CurrentFrame + 1, TotalFrames);
        }

        public void SyncStrokesToFrame()
        {
            if (CurrentFrame < 0 || CurrentFrame >= _frames.Count) return;

            _frames[CurrentFrame].Strokes = _strokes.Select(s => s.Clone()).ToList();
        }

        public void SyncFrameToStrokes()
        {
            if (CurrentFrame < 0 || CurrentFrame >= _frames.Count) return;

            _strokes = _frames[CurrentFrame].Strokes
                .Select(s => s.Clone()).ToList();
        }

        public void AddEmptyFrame()
        {
            _frames.Add(new Frame());
        }

        public void ClearAll()
        {
            _frames.Clear();
            _strokes.Clear();
            SetStrokes(_strokes);
            CurrentFrame = 0;
        }

        public List<Frame> GetAllFrames() => _frames;

        private void DrawFrameIfExists(int i, IBrush color, DrawingContext context)
        {
            var frames = _frames;
            if (i < 0 || i >= frames.Count) return;

            var f = frames[i];
            foreach (var stroke in f.Strokes)
            {
                _strokeRenderer.DrawStrokeWithColorOverride(stroke, 0, color, context);
            }
        }

        public void DuplicateFrame(Control canvas)
        {
            int cur = CurrentFrame;
            if (cur < 0 || cur >= _frames.Count) return;

            var cloned = _frames[cur].Strokes.Select(s => s.Clone()).ToList();
            var newFrame = new Frame { Strokes = cloned };
            _frames.Insert(cur + 1, newFrame);
            LoadFrame(cur + 1, canvas);
        }
        public void NextFrame(Visual visual)
        {
            SaveCurrentFrame();
            int next = CurrentFrame + 1;

            if (next >= TotalFrames)
                AddEmptyFrame();

            LoadFrame(next, visual);
        }
        public void PreviousFrame(Visual visual)
        {
            SaveCurrentFrame();
            if (CurrentFrame > 0) LoadFrame(CurrentFrame - 1, visual);
        }
        public async void Play(Visual visual)
        {
            if (TotalFrames == 0) return;

            _isPlaying = true;
            while (_isPlaying)
            {
                int next = (CurrentFrame + 1) % TotalFrames;
                LoadFrame(next, visual);
                await Task.Delay(150);
            }
        }
        public void Stop() => _isPlaying = false;
        public void TogglePlay(Visual visual)
        {
            if (_isPlaying) Stop();
            else Play(visual);
        }
    }
}
