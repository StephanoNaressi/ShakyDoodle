using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using ShakyDoodle.Controllers;
using ShakyDoodle.Models;
using ShakyDoodle.Utils;

namespace ShakyDoodle.Services
{
    public class FileService
    {
        private readonly FrameController _frameController;

        public FileService(FrameController frameController)
        {
            _frameController = frameController;
        }

        public void SaveProject(string filePath)
        {
            var project = new SerializableProject
            {
                CurrentFrame = _frameController.CurrentFrame,
                ActiveLayerIndex = _frameController.ActiveLayerIndex,
                Frames = _frameController.GetAllFrames().Select(ConvertFrame).ToList()
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(project, options);
            File.WriteAllText(filePath, json);
        }

        public void LoadProject(string filePath)
        {
            if (!File.Exists(filePath)) return;

            var json = File.ReadAllText(filePath);
            var project = JsonSerializer.Deserialize<SerializableProject>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (project == null) return;

            _frameController.ClearAll(false);

            foreach (var serializableFrame in project.Frames)
            {
                var frame = ConvertFrame(serializableFrame);
                _frameController.AddFrame(frame);
            }

            _frameController.LoadFrame(Math.Min(project.CurrentFrame, _frameController.TotalFrames - 1));
            _frameController.ActiveLayerIndex = project.ActiveLayerIndex;
        }

        private SerializableFrame ConvertFrame(Frame frame)
        {
            return new SerializableFrame
            {
                Layers = frame.Layers.Select(ConvertLayer).ToList()
            };
        }

        private Frame ConvertFrame(SerializableFrame serializableFrame)
        {
            return new Frame
            {
                Layers = serializableFrame.Layers.Select(ConvertLayer).ToList(),
                IsDirty = true
            };
        }

        private SerializableLayer ConvertLayer(Layer layer)
        {
            return new SerializableLayer
            {
                Opacity = layer.Opacity,
                Name = layer.Name,
                IsVisible = layer.IsVisible,
                Strokes = layer.Strokes.Select(ConvertStroke).ToList()
            };
        }

        private Layer ConvertLayer(SerializableLayer serializableLayer)
        {
            return new Layer
            {
                Opacity = serializableLayer.Opacity,
                Name = serializableLayer.Name,
                IsVisible = serializableLayer.IsVisible,
                Strokes = serializableLayer.Strokes.Select(ConvertStroke).ToList(),
                IsDirty = true
            };
        }

        private SerializableStroke ConvertStroke(Stroke stroke)
        {
            return new SerializableStroke
            {
                CreatedAt = stroke.CreatedAt,
                Points = stroke.Points.Select(p => new SerializablePoint { X = p.X, Y = p.Y }).ToList(),
                Pressures = stroke.Pressures.ToList(),
                Color = new SerializableColor { A = stroke.Color.A, R = stroke.Color.R, G = stroke.Color.G, B = stroke.Color.B },
                Size = stroke.Size,
                Alpha = stroke.Alpha,
                PenLineCap = stroke.PenLineCap,
                Shake = stroke.Shake,
                BrushType = stroke.BrushType
            };
        }

        private Stroke ConvertStroke(SerializableStroke serializableStroke)
        {
            var color = Color.FromArgb(
                serializableStroke.Color.A,
                serializableStroke.Color.R,
                serializableStroke.Color.G,
                serializableStroke.Color.B
            );

            var points = serializableStroke.Points.Select(p => new Point(p.X, p.Y)).ToList();

            return new Stroke(
                color,
                points,
                serializableStroke.Size,
                serializableStroke.Alpha,
                serializableStroke.PenLineCap,
                serializableStroke.Pressures,
                serializableStroke.Shake,
                serializableStroke.BrushType
            );
        }
    }
}