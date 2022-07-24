using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;

namespace GdiHook.DTO
{
    public class ControlElement
    {
        public IntPtr HWnd { get; set; }
        public ControlElement[] Children { get; set; }
        public int ProcessId { get; set; }
        public Rectangle WindowRect { get; set; }
        public Rectangle ScreenClientRect { get; set; }
        public Rectangle ClientRect { get; set; }
        public List<ControlText> Items { get; set; } = new List<ControlText>();

        public Point ClientOffset()
            => new Point(WindowRect.X - ClientRect.X, WindowRect.Y - ClientRect.Y);

        public void Add(int x, int y, string text, Rectangle? rect)
            => Items.Add(new ControlText() { X = x, Y = y, Text = text, Rect = rect });

        public void Add(int x, int y, string text)
            => Add(x, y, text, null);

        public override string ToString()
            => $"{{HWnd={HWnd}" + (Children.Any() ? $", Children=[{string.Join(',', Children.ToString())}]" : "") + "}";


        private IEnumerable<KeyValuePair<IntPtr, ControlElement>> ToKeyValuePair()
        {
            KeyValuePair<IntPtr, ControlElement>[] result = { new KeyValuePair<IntPtr, ControlElement>(HWnd, this) };
            return result.Concat(Children.SelectMany(d => d.ToKeyValuePair()));
        }

        public Dictionary<IntPtr, ControlElement> ToDictionary()
        {
            var items = ToKeyValuePair();
            return ToKeyValuePair()
                .Select(k => k.Key)
                .Distinct()
                .ToDictionary(k => k, k => items.First(i => i.Key == k).Value);
        }


        public IEnumerable<ControlText> GrabTextScreen()
        {
            var point = ClientOffset();
            return Items
                .Select(t => t.Offset(point))
                .Concat(Children.SelectMany(gd => gd.GrabTextScreen()));
        }
    }
}
