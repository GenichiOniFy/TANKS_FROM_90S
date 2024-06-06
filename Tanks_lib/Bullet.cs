using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Tanks_lib
{
    public class Bullet
    {
        #region Свойства
        public double X { get; set; }
        public double Y { get; set; }
        public double SpeedX { get; set; }
        public double SpeedY { get; set; }
        public Image Texture { get; set; }
        #endregion

        #region Конструктор пули
        public Bullet(double x, double y, double speed, double angle)
        {
            X = x;
            Y = y;
            SpeedX = speed * Math.Cos(angle * Math.PI / 180);
            SpeedY = speed * Math.Sin(angle * Math.PI / 180);
            Texture = new Image
            {
                Width = 10,
                Height = 10,
                Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Bullet.png"))
            };
        }
        #endregion

        #region Методы

        #region Движение пули
        public void Move()
        {
            X+= SpeedX;
            Y+= SpeedY;
        }
        #endregion

        #region Проверка столкновения со стеной
        public List<Wall> CheckCollisionWithWall(List<Wall> walls)
        {
            foreach (var wall in walls)
            {
                Rect bulletRect = new Rect(X, Y, Texture.Width, Texture.Height);
                Rect wallRect = new Rect(Canvas.GetLeft(wall.Texture), Canvas.GetTop(wall.Texture), wall.Width, wall.Height);

                if (bulletRect.IntersectsWith(wallRect))
                {
                    List<Wall> neig = new List<Wall>();
                    if ((SpeedX>=0.001 || SpeedX <= -0.001) && (SpeedY<=0.001 || SpeedY >= -0.001))
                    {
                        neig = wall.GetVerticalNeighbor(30, walls);
                    }
                    else
                    {
                        neig = wall.GetHorizontalNeighbor(30, walls);
                    }
                    
                    
                    neig.Add(wall);
                    return neig;
                }
            }
            return null!; // Нет столкновения
        }
        #endregion

        #region Проверка столкновения с игроком
        public bool CheckCollisionWithPlayer(Player tank)
        {
            Rect bulletRect = new Rect(X, Y, Texture.Width, Texture.Height);
            Rect playerRect = new Rect(Canvas.GetLeft(tank.Texture), Canvas.GetTop(tank.Texture), tank.Texture.Width, tank.Texture.Height);
            return bulletRect.IntersectsWith(playerRect);
        }
        #endregion

        #endregion
    }
}
