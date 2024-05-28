using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Tanks_lib
{
    public class Player
    {
        #region Свойства и поля
        private Canvas GameCanvas;
        private List<Wall> walls;
        private RotateTransform tankRotateTransform;
        public int Score { get; set; }
        public double Speed { get; set; }
        public Image Texture { get; set; }
        private DateTime lastShotTime;
        #endregion

        #region Конструктор игрока
        public Player(Canvas gameCanvas, double speed, List<Wall> walls)
        {
            Score = 0;
            GameCanvas = gameCanvas;
            this.walls = walls;
            Speed = speed;
            Texture = new Image
            {
                Width = 70,
                Height = 70,
                Source = new BitmapImage(new Uri("pack://application:,,,/Resources/tank.png"))
            };
            tankRotateTransform = new RotateTransform(0);
            Texture.RenderTransform = tankRotateTransform;
            Texture.RenderTransformOrigin=new System.Windows.Point(0.5,0.5); //танк будет вращаться вокруг своего центра.
        }
        #endregion

        #region Методы

        #region Условия движения танка
        public void MoveTank(bool isMovingUp, bool isMovingDown, bool isMovingLeft, bool isMovingRight)
        {
            double left = Canvas.GetLeft(Texture);//X
            double top = Canvas.GetTop(Texture);//Y

            #region Если одновременно нажаты вертикальная и горизонтальная клавиши, не двигаться
            bool isVerticalMovement = isMovingUp || isMovingDown;
            bool isHorizontalMovement = isMovingLeft || isMovingRight;

            if (isVerticalMovement && isHorizontalMovement)
            {
                return;
            }
            #endregion

            double tolerance = 20.0;//Погрешность для смещения

            #region Вверх
            if (isMovingUp)
            {
                if (top > 0 && !CheckWallCollision(left, top - Speed, Texture.Width, Texture.Height))
                {
                    Canvas.SetTop(Texture, top - Speed);
                    tankRotateTransform.Angle = 0;
                }
                else
                {
                    AdjustTankPosition(tolerance, -Speed, 0);
                }
            }
            #endregion

            #region Вниз
            else if (isMovingDown)
            {
                if (top + Texture.Height < GameCanvas.ActualHeight && !CheckWallCollision(left, top + Speed, Texture.Width, Texture.Height))
                {
                    Canvas.SetTop(Texture, top + Speed);
                    tankRotateTransform.Angle = 180;
                }
                else
                {
                    AdjustTankPosition(tolerance, Speed, 0);
                }
            }
            #endregion

            #region Влево
            else if (isMovingLeft)
            {
                if (left > 0 && !CheckWallCollision(left - Speed, top, Texture.Width, Texture.Height))
                {
                    Canvas.SetLeft(Texture, left - Speed);
                    tankRotateTransform.Angle = 270;
                }
                else
                {
                    AdjustTankPosition(tolerance, 0, -Speed);
                }
            }
            #endregion

            #region Вправо
            else if (isMovingRight)
            {
                if (left + Texture.Width < GameCanvas.ActualWidth && !CheckWallCollision(left + Speed, top, Texture.Width, Texture.Height))
                {
                    Canvas.SetLeft(Texture, left + Speed);
                    tankRotateTransform.Angle = 90;
                }
                else
                {
                    AdjustTankPosition(tolerance, 0, Speed);
                }
            }
            #endregion
        }
        
        #region Регулировка позиции танка
        private void AdjustTankPosition(double tolerance, double verticalAdjustment, double horizontalAdjustment)
        {
            double left = Canvas.GetLeft(Texture);
            double top = Canvas.GetTop(Texture);

            if (horizontalAdjustment != 0)
            {
                // Попробовать сместить танк вверх или вниз
                for (double offset = 1; offset <= tolerance; offset++)
                {
                    if (!CheckWallCollision(left + horizontalAdjustment, top - offset, Texture.Width, Texture.Height))
                    {
                        Canvas.SetTop(Texture, top - offset);
                        Canvas.SetLeft(Texture, left + horizontalAdjustment);
                        return;
                    }
                    if (!CheckWallCollision(left + horizontalAdjustment, top + offset, Texture.Width, Texture.Height))
                    {
                        Canvas.SetTop(Texture, top + offset);
                        Canvas.SetLeft(Texture, left + horizontalAdjustment);
                        return;
                    }
                }
            }

            if (verticalAdjustment != 0)
            {
                // Попробовать сместить танк влево или вправо
                for (double offset = 1; offset <= tolerance; offset++)
                {
                    if (!CheckWallCollision(left - offset, top + verticalAdjustment, Texture.Width, Texture.Height))
                    {
                        Canvas.SetLeft(Texture, left - offset);
                        Canvas.SetTop(Texture, top + verticalAdjustment);
                        return;
                    }
                    if (!CheckWallCollision(left + offset, top + verticalAdjustment, Texture.Width, Texture.Height))
                    {
                        Canvas.SetLeft(Texture, left + offset);
                        Canvas.SetTop(Texture, top + verticalAdjustment);
                        return;
                    }
                }
            }
        }

        public bool CheckWallCollision(double x, double y, double width, double height)
        {
            double borderWidth = 90; // Ширина рамки

            if (x < borderWidth || x + width > GameCanvas.ActualWidth - borderWidth ||
                y < borderWidth || y + height > GameCanvas.ActualHeight - borderWidth)
            {
                return true; // Столкновение с рамкой
            }
            foreach (var wall in walls)
            {
                Rect wallRect = new Rect(Canvas.GetLeft(wall.Texture), Canvas.GetTop(wall.Texture), wall.Texture.Width, wall.Texture.Height);
                Rect playerRect = new Rect(x, y, Texture.Width, Texture.Height);

                if (wallRect.IntersectsWith(playerRect))
                {
                    return true; // Столкновение с стеной
                }
            }
            return false; // Нет стены на пути
        }
        #endregion

        #endregion

        #region Выстрел
        public void Shoot(ref List<Bullet> bullets, int speed)
        {
            // Проверяем, можно ли сейчас стрелять
            if (!((DateTime.Now-lastShotTime).TotalSeconds>1))
                return;

            // Перезарядка пушки: обновляем время последнего выстрела и блокируем стрельбу
            lastShotTime = DateTime.Now;
            double tankX = Canvas.GetLeft(Texture) + Texture.Width / 2-5;
            double tankY = Canvas.GetTop(Texture) + Texture.Height / 2-5;
            double angle = tankRotateTransform.Angle;

            Bullet bullet = new Bullet(tankX, tankY, speed, angle - 90);
            bullets.Add(bullet);
            Canvas.SetLeft(bullet.Texture, tankX);
            Canvas.SetTop(bullet.Texture, tankY);
            GameCanvas.Children.Add(bullet.Texture);
        }
        #endregion

        #endregion
    }
}
