﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ShakyDoodle.Models;
using ShakyDoodle.Rendering;
using ShakyDoodle.Utils;

namespace ShakyDoodle.Controllers
{
    public class FrameController
    {
        private StrokeRenderer _strokeRenderer;
        public int CurrentFrame;
        public int TotalFrames => _frames.Count;
        public int TotalLayers => _frames[CurrentFrame].Layers.Count;
        private List<Frame> _frames = new();
        private int _activeLayerIndex = 0;
        public int ActiveLayerIndex
        {
            get => _activeLayerIndex;
            set => _activeLayerIndex = value;
        }
        public Action<int, int>? OnFrameChanged;
        public Action<int, int>? OnLayerChanged;
        public Action? OnInvalidateRequested;
        public RenderTargetBitmap? CachedBitmap;
        public bool IsDirty = true;
        private bool _isPlaying = false;
        public bool IsLocked { get; private set; }

        public Action<bool>? OnLockStateChanged;

        public FrameController(Rect bounds, StrokeRenderer strokeRenderer, int startingFrame = 0)
        {
            _strokeRenderer = strokeRenderer;
            CurrentFrame = startingFrame;
        }

        public void ToggleLock()
        {
            IsLocked = !IsLocked;
            OnLockStateChanged?.Invoke(IsLocked);
        }

        public void EraseStrokes(List<Stroke> strokes, Point eraserCenter, double eraserRadius)
        {
            strokes.RemoveAll(stroke =>
                stroke.Points.Any(p => MathExtras.Instance.Distance(p, eraserCenter) <= eraserRadius)
            );
        }
        public List<Stroke> GetStrokes()
        {
            if (CurrentFrame < 0 || CurrentFrame >= _frames.Count) return new List<Stroke>();
            var layers = _frames[CurrentFrame].Layers;
            if (_activeLayerIndex < 0 || _activeLayerIndex >= layers.Count) return new List<Stroke>();
            return layers[_activeLayerIndex].Strokes;
        }

        public void SetStrokes(List<Stroke> strokes)
        {
            if (CurrentFrame < 0 || CurrentFrame >= _frames.Count) return;

            var layers = _frames[CurrentFrame].Layers;
            if (_activeLayerIndex < 0 || _activeLayerIndex >= layers.Count) return;
            layers[_activeLayerIndex].IsDirty = true;
            layers[_activeLayerIndex].Strokes = strokes.Select(s => s.Clone()).ToList();
            _frames[CurrentFrame].IsDirty = true;
        }

        public void SaveCurrentFrame()
        {
            var strokesCopy = GetStrokes().Select(s => s.Clone()).ToList();

            if (CurrentFrame < 0 || CurrentFrame >= _frames.Count) return;

            var layers = _frames[CurrentFrame].Layers;
            if (_activeLayerIndex < 0 || _activeLayerIndex >= layers.Count) return;

            layers[_activeLayerIndex].Strokes = strokesCopy;
            _frames[CurrentFrame].IsDirty = true;
            _frames[CurrentFrame].CachedBitmap = null;
        }

        public void DeleteCurrentFrame()
        {
            if (IsLocked || _frames.Count <= 1) return;
            _frames.RemoveAt(CurrentFrame);
            if (CurrentFrame >= _frames.Count) CurrentFrame = _frames.Count - 1;
            MarkDirty();
            OnFrameChanged?.Invoke(CurrentFrame + 1, _frames.Count);
            IsDirty = true;
        }
        public void UpdateLayerOpacity(double val)
        {
            if (CurrentFrame < 0 || CurrentFrame >= _frames.Count)
                return;

            var layers = _frames[CurrentFrame].Layers;
            if (ActiveLayerIndex < 0 || ActiveLayerIndex >= layers.Count)
                return;

            var layer = layers[ActiveLayerIndex];
            layer.Opacity = val;
            _frames[CurrentFrame].IsDirty = true;
            _frames[CurrentFrame].CachedBitmap = null;

            OnInvalidateRequested?.Invoke();
        }


        public void DeleteCurrentLayer()
        {
            if (CurrentFrame < 0) return;

            var layers = _frames[CurrentFrame].Layers;

            if (ActiveLayerIndex < 0 || ActiveLayerIndex >= layers.Count) return;
            if (layers.Count <= 1) return;

            layers.RemoveAt(ActiveLayerIndex);

            if (ActiveLayerIndex >= layers.Count)
                ActiveLayerIndex = layers.Count - 1;

            _frames[CurrentFrame].IsDirty = true;
            MarkDirty();
            OnLayerChanged?.Invoke(ActiveLayerIndex + 1, layers.Count);
        }

        public void LoadFrame(int index)
        {
            if (index < 0 || index >= _frames.Count) return;

            CurrentFrame = index;
            _frames[CurrentFrame].CachedBitmap = null;
            _frames[CurrentFrame].IsDirty = true;
            SyncFrameToStrokes();
            OnInvalidateRequested?.Invoke();

            OnFrameChanged?.Invoke(CurrentFrame + 1, TotalFrames);
        }

        public void SyncStrokesToFrame()
        {
            var strokes = GetStrokes();
            SetStrokes(strokes);
        }

        public void SyncFrameToStrokes()
        {
            var strokes = GetStrokes().Select(s => s.Clone()).ToList();
            SetStrokes(strokes);
        }

        public void AddEmptyFrame()
        {
            if (IsLocked) return;
            var layers = new List<Layer>();
            for (int i = 0; i < 5; i++)
            {
                layers.Add(new Layer
                {
                    Name = $"Layer {i + 1}",
                    IsVisible = true,
                    Strokes = new List<Stroke>()
                });
            }
            _frames.Add(new Frame { Layers = layers });
            ActiveLayerIndex = 4;
            OnLayerChanged?.Invoke(ActiveLayerIndex + 1, layers.Count);
        }

        public void SetCurrentLayer(int index)
        {
            if (index < 0 || index > 9) return;
            if (CurrentFrame < 0 || CurrentFrame >= _frames.Count) return;
            var layers = _frames[CurrentFrame].Layers;
            while (layers.Count <= index)
            {
                AddLayerToCurrentFrame($"Layer {layers.Count + 1}");
            }
            ActiveLayerIndex = index;
            OnLayerChanged?.Invoke(ActiveLayerIndex + 1, layers.Count);
        }
        public void AddLayerToCurrentFrame(String name)
        {
            if (CurrentFrame < 0 || CurrentFrame >= _frames.Count) return;
            _frames[CurrentFrame].Layers.Add(new Layer
            {
                Name = name,
                IsVisible = true,
                Strokes = new List<Stroke>()
            });
            _frames[CurrentFrame].IsDirty = true;
            MarkDirty();
        }

        public void ClearAll(bool addEmpty = true)
        {
            if (IsLocked)
            {
                IsLocked = false;
                OnLockStateChanged?.Invoke(IsLocked);
            }
            
            _frames.Clear();
            if(addEmpty) AddEmptyFrame();
            CurrentFrame = 0;
            ActiveLayerIndex = 4; 

            IsDirty = true;
            CachedBitmap = null;
            _strokeRenderer.ClearCaches();
            foreach (var frame in _frames)
            {
                frame.CachedBitmap = null;
                frame.IsDirty = true;
            }
        }
        public List<Frame> GetAllFrames() => _frames;
        public List<Stroke> GetAllVisibleStrokes()
        {
            if (CurrentFrame < 0 || CurrentFrame >= _frames.Count)
                return new List<Stroke>();

            return _frames[CurrentFrame].Layers
                   .Where(l => l.IsVisible)
                   .SelectMany(l => l.Strokes)
                   .ToList();
        }
        public void DuplicateFrame()
        {
            if (IsLocked) return;
            int cur = CurrentFrame;
            if (cur < 0 || cur >= _frames.Count) return;

            var originalFrame = _frames[cur];
            var newFrame = new Frame
            {
                Layers = originalFrame.Layers
                    .Select(layer => new Layer
                    {
                        Name = layer.Name,
                        IsVisible = layer.IsVisible,
                        Strokes = layer.Strokes.Select(s => s.Clone()).ToList()
                    }).ToList(),
                IsDirty = true
            };

            _frames.Insert(cur + 1, newFrame);
            LoadFrame(cur + 1);
        }
        public void NextFrame()
        {
            SaveCurrentFrame();
            int next = CurrentFrame + 1;

            if (next >= TotalFrames)
            {
                if (IsLocked)
                {
                    // loop back to the beginning if locked
                    next = 0;
                }
                else
                {
                    AddEmptyFrame();
                }
            }
            LoadFrame(next);
        }
        public void PreviousFrame()
        {
            SaveCurrentFrame();
            if (CurrentFrame > 0) LoadFrame(CurrentFrame - 1);
        }
        public async void Play()
        {
            if (TotalFrames == 0) return;
            SetCurrentLayer(0);
            _isPlaying = true;
            while (_isPlaying)
            {
                int next = (CurrentFrame + 1) % TotalFrames;
                LoadFrame(next);
                await Task.Delay(150);
            }
        }
        public void Stop() => _isPlaying = false;
        public void TogglePlay()
        {
            if (_isPlaying) Stop();
            else Play();
        }
        public void MarkDirty()
        {
            if (CurrentFrame >= 0 && CurrentFrame < _frames.Count)
            {
                _frames[CurrentFrame].IsDirty = true;
                CachedBitmap = null;
                OnInvalidateRequested?.Invoke();
            }
        }
        public Layer? GetCurrentLayer()
        {
            if (CurrentFrame < 0 || CurrentFrame >= _frames.Count)
                return null;

            var layers = _frames[CurrentFrame].Layers;

            if (_activeLayerIndex < 0 || _activeLayerIndex >= layers.Count)
                return null; 

            return layers[_activeLayerIndex];
        }

        public void AddFrame(Frame frame)
        {
            _frames.Add(frame);
            OnFrameChanged?.Invoke(CurrentFrame + 1, TotalFrames);
        }

        public void SetFrames(List<Frame> frames)
        {
            _frames.Clear();
            _frames.AddRange(frames);
            CurrentFrame = 0;
            ActiveLayerIndex = _frames.Count > 0 && _frames[0].Layers.Count > 0 ? 0 : 0;
            
            CachedBitmap = null;
            _strokeRenderer.ClearCaches();
            
            foreach (var frame in _frames)
            {
                frame.CachedBitmap = null;
                frame.IsDirty = true;
                foreach (var layer in frame.Layers)
                {
                    layer.CachedBitmap = null;
                    layer.IsDirty = true;
                }
            }
            
            OnFrameChanged?.Invoke(CurrentFrame + 1, TotalFrames);
            OnInvalidateRequested?.Invoke();
        }
    }
}
