using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

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

        public bool life;
        #endregion

        #region Конструктор стены
        public Wall(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            life= true;
            Texture = new Image
            {
                Width = width,
                Height = height,
                Source = new BitmapImage(new Uri("pack://application:,,,/Resources/wall.png")) // Путь к изображению стены
            };

            Canvas.SetLeft(Texture, x);
            Canvas.SetTop(Texture, y);
        }

        public void Break()
        {
            Texture = new Image
            {
                Width = Width,
                Height = Height,
                Source = new BitmapImage(new Uri("pack://application:,,,/Resources/broken_wall.png")) // Путь к изображению стены
            };
            life = false;
        }

        #region Методы для получения соседей
        public List<Wall> GetVerticalNeighbor(double yOffset, List<Wall> walls)
        {
            double targetY1 = Y + yOffset;
            double targetY2 = Y - yOffset;
            List<Wall> w= new List<Wall>();
            foreach (var wall in walls)
            {
                if (wall.X == X && (wall.Y == targetY1 || wall.Y==targetY2))
                {
                    w.Add(wall);
                    if (w.Count == 2)
                    {
                        return w;
                    }
                }
            }
            return w;
        }

        public List<Wall> GetHorizontalNeighbor(double xOffset, List<Wall> walls)
        {
            double targetX1 = X + xOffset;
            double targetX2 = X - xOffset;
            List<Wall> w = new List<Wall>();
            foreach (var wall in walls)
            {
                if (wall.Y == Y && (wall.X == targetX1 || wall.X == targetX2))
                {
                    w.Add(wall);
                    if (w.Count == 2)
                    {
                        return w;
                    }
                }
            }
            return w;
        }
        #endregion

        #endregion
    }
}
