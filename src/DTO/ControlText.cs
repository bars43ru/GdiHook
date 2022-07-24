using System.Drawing;

namespace GdiHook.DTO
{
    /// <summary>
    /// Представление текстовой информации на экране
    /// </summary>
    public struct ControlText
    {
        /// <summary>
        /// Координаты начала отрисовки левого угла текста
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Координаты начала отрисовки верхнего угла текста
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Текст отправленный на отрисовку
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Прямоугольник в который должен быть в писан текст
        /// </summary>
        public Rectangle? Rect { get; set; }

        public Point Point()
            => new Point(X, Y);

        public ControlText Offset(Point point)
        {
            ControlText result = new();
            result.X += X + point.X;
            result.Y += Y + point.Y;
            result.Text = Text;
            if (Rect.HasValue)
            {
                Rectangle rect = Rect.Value;
                rect.Offset(point);
                result.Rect = rect;
            }
            return result;
        }
    }
}
