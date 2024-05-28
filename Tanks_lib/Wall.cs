using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Tanks_lib
{
    public class Wall
    {
        #region Поля и свойства
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Image Texture { get; set; }
        #endregion

        #region Конструктор стены
        public Wall(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;

            Texture = new Image
            {
                Width = width,
                Height = height,
                Source = new BitmapImage(new Uri("pack://application:,,,/Resources/wall.png")) // Путь к изображению стены
            };

            Canvas.SetLeft(Texture, x);
            Canvas.SetTop(Texture, y);
        }
        #endregion
    }
}
