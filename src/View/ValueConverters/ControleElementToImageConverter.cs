using GdiHook.DTO;
using Newtonsoft.Json;
using System;
using System.Windows.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.IO;

namespace GdiHook.View.ValueConverters
{
    public class ControleElementToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            return BitmapToImageSource(Paint((ControlElement)value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }

        readonly Font _font = new Font("Times New Roman", 10.0f);
        private Bitmap Paint(ControlElement controlElement)
        {
            var bitmap = new Bitmap(controlElement.WindowRect.Width, controlElement.WindowRect.Height, PixelFormat.Format32bppArgb);
            try
            {
                using var graphics = Graphics.FromImage(bitmap);
                PaintClient(controlElement, graphics, 0, 0);
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }
            return bitmap;
        }

        private void PaintClient(ControlElement controlElement, Graphics graphics, int deltaX, int deltaY)
        {
            Rectangle GetNewRectangle(Rectangle rectangle, int deltaX, int deltaY)
                => new Rectangle(rectangle.X + deltaX, rectangle.Y + deltaY, rectangle.Width, rectangle.Height);

            // рабочая область
            var yellowPen = new Pen(Color.Yellow, 1);
            graphics.DrawRectangle(yellowPen, GetNewRectangle(controlElement.ClientRect, deltaX, deltaY));

            var redPen = new Pen(Color.Red, 1);
            foreach (var item in controlElement.Items)
            {
                RectangleF rectangleF;
                if (item.Rect.HasValue)
                {
                    var clientRect = GetNewRectangle(item.Rect.Value, deltaX, deltaY);
                    graphics.DrawRectangle(redPen, clientRect);
                    rectangleF = new RectangleF(clientRect.Left, clientRect.Top, clientRect.Width, clientRect.Height);
                }
                else
                {
                    rectangleF = new RectangleF(item.X + deltaX, item.Y + deltaY, controlElement.ClientRect.Width, controlElement.ClientRect.Height);
                }
                //graphics.DrawString(item.Text, new Font("Times New Roman", 10.0f), Brushes.White, item.X + deltaX, item.Y + deltaY, rectangleF);
                graphics.DrawString(item.Text, _font, Brushes.White, rectangleF);
            }
            foreach (var child in controlElement.Children)
            {
                PaintClient(child, graphics, child.WindowRect.Left - controlElement.WindowRect.Left, child.WindowRect.Top - controlElement.WindowRect.Top);
            }
        }

        private void PaintDesktop(ControlElement controlElement, Graphics graphics)
        {
            // окно
            graphics.DrawRectangle(new Pen(Color.Green, 1), controlElement.WindowRect);
            // рабочая область
            graphics.DrawRectangle(new Pen(Color.Yellow, 1), controlElement.ScreenClientRect);

            // текст контрола
            var redPen = new Pen(Color.Red, 1);
            foreach (var item in controlElement.Items)
            {
                var offsetItem = item.Offset(controlElement.ClientOffset());
                if (offsetItem.Rect.HasValue)
                {
                    graphics.DrawRectangle(redPen, offsetItem.Rect.Value);
                    graphics.DrawString(offsetItem.Text, _font, Brushes.White, offsetItem.Rect.Value);
                }
                else
                {
                    graphics.DrawString(offsetItem.Text, _font, Brushes.White, offsetItem.Point());
                }
            }
            foreach (var child in controlElement.Children)
            {
                PaintDesktop(child, graphics);
            }
        }

        public Bitmap PaintWindow(ControlElement controlElement)
        {
            var bitmap = new Bitmap(controlElement.WindowRect.X + controlElement.WindowRect.Width + 5, controlElement.WindowRect.Y + controlElement.WindowRect.Height + 5, PixelFormat.Format32bppArgb);
            try
            {
                using var graphics = Graphics.FromImage(bitmap);
                PaintDesktop(controlElement, graphics);
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }
            return bitmap;
        }
    }
}
