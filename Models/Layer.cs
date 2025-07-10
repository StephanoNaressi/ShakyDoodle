using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShakyDoodle.Models
{
    public class Layer
    {
        public string Name;
        public bool IsVisible = true;
        public List<Stroke> Strokes = new();
    }
}
