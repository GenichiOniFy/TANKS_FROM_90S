using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Tanks_lib
{
    public class Enemy
    {
        #region Свойства и поля
        public double X { get; set; }
        public double Y { get; set; }
        public double Speed { get; set; }
        public Image Texture { get; set; }

        private DateTime lastShotTime;

        private double CanvasWidth, CanvasHeight;

        private double currentAngle;

        private int moveCooldown;
        private const int MaxCooldown = 30; // Количество шагов до смены направления
        #endregion

        #region Конструктор
        public Enemy(double x, double y, double speed, double a,double b)
        {
            X = x;
            Y = y;
            CanvasWidth = a; CanvasHeight = b;
            Speed = speed;
            currentAngle = 0; // Инициализируем текущий угол
            Texture = new Image
            {
                Width = 70,
                Height = 70,
                Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Enemy.png"))
            };
        }
        #endregion

        #region Методы

        #region Перемещение врага

        #region Метод перемещения врага к игроку, избегая столкновения со стенами. Работает как-то не очень
        public void MoveTowardsPlayer(double playerX, double playerY, List<Wall> walls)
        {
            #region Проверка кулдауна: Если кулдаун больше нуля, уменьшаем его и продолжаем движение врага в текущем направлении.
            if (moveCooldown > 0)
            {
                moveCooldown--;
                MoveInCurrentDirection(walls);
                return;
            }
            #endregion

            #region Определяем расстояние по осям x и y между существом и игроком.
            double dx = playerX - X;
            double dy = playerY - Y;
            #endregion

            #region Выбор направления движения, проверяем столкновения. Перемещаем
            if (Math.Abs(dx) > Math.Abs(dy))
            {
                if (dx > 0 && !CheckWallCollision(X + Speed, Y, walls))
                {
                    X += Speed;
                    Y = Math.Round(Y);
                    RotateImage(90);
                    currentAngle = 90;
                }
                else if (dx < 0 && !CheckWallCollision(X - Speed, Y, walls))
                {
                    X -= Speed;
                    Y = Math.Round(Y);
                    RotateImage(270);
                    currentAngle = 270;
                }
                else if (!CheckWallCollision(X, Y + Speed, walls) && !CheckWallCollision(X, Y - Speed, walls))
                {
                    if (dy > 0)
                    {
                        Y += Speed;
                        X = Math.Round(X);
                        RotateImage(180);
                        currentAngle = 180;
                    }
                    else
                    {
                        Y -= Speed;
                        X = Math.Round(X);
                        RotateImage(0);
                        currentAngle = 0;
                    }
                }
            }
            else
            {
                if (dy > 0 && !CheckWallCollision(X, Y + Speed, walls))
                {
                    Y += Speed;
                    X = Math.Round(X);
                    RotateImage(180);
                    currentAngle = 180;
                }
                else if (dy < 0 && !CheckWallCollision(X, Y - Speed, walls))
                {
                    Y -= Speed;
                    X = Math.Round(X);
                    RotateImage(0);
                    currentAngle = 0;
                }
                else if (!CheckWallCollision(X + Speed, Y, walls) && !CheckWallCollision(X - Speed, Y, walls))
                {
                    if (dx > 0)
                    {
                        X += Speed;
                        Y = Math.Round(Y);
                        RotateImage(90);
                        currentAngle = 90;
                    }
                    else
                    {
                        X -= Speed;
                        Y = Math.Round(Y);
                        RotateImage(270);
                        currentAngle = 270;
                    }
                }
            }
            #endregion

            #region Обновляем кулдаун
            moveCooldown = MaxCooldown;
            #endregion
        }
        #endregion

        #region метод продолжает движение в текущем направлении,
        private void MoveInCurrentDirection(List<Wall> walls)
        {
            switch (currentAngle)
            {
                case 0: // Вверх
                    if (!CheckWallCollision(X, Y - Speed, walls)) Y -= Speed;
                    break;
                case 180: // Вниз
                    if (!CheckWallCollision(X, Y + Speed, walls)) Y += Speed;
                    break;
                case 90: // Вправо
                    if (!CheckWallCollision(X + Speed, Y, walls)) X += Speed;
                    break;
                case 270: // Влево
                    if (!CheckWallCollision(X - Speed, Y, walls)) X -= Speed;
                    break;
            }
        }
        #endregion

        #region Проверка столкновения с рамкой или стеной
        private bool CheckWallCollision(double x, double y, List<Wall> walls)
        {

            double borderWidth = 90;

            if (x < borderWidth || x + Texture.Width > CanvasWidth - borderWidth ||
                y < borderWidth || y + Texture.Height > CanvasHeight - borderWidth)
            {
                return true; // Столкновение с рамкой
            }
            foreach (var wall in walls)
            {
                Rect wallRect = new Rect(Canvas.GetLeft(wall.Texture), Canvas.GetTop(wall.Texture), wall.Texture.Width, wall.Texture.Height);
                Rect enemyRect = new Rect(x, y, Texture.Width, Texture.Height);

                if (wallRect.IntersectsWith(enemyRect))
                {
                    return true; // Стена найдена на пути
                }
            }
            return false; // Нет стены на пути
        }
        #endregion

        #region Поворот врага на указанный угол
        private void RotateImage(double angle)
        {
            var rotation = new RotateTransform(angle)
            {
                CenterX = Texture.Width / 2,
                CenterY = Texture.Height / 2
            };

            Texture.RenderTransform = rotation;
        }
        #endregion

        #region Проверка смотрит ли враг на игрока
        public bool IsFacingPlayer(double playerX, double playerY)
        {
            double dx = playerX - X;
            double dy = playerY - Y;

            switch (currentAngle)
            {
                case 0: // Вверх
                    return Math.Abs(dx) < Texture.Width / 2 && dy < 0;
                case 180: // Вниз
                    return Math.Abs(dx) < Texture.Width / 2 && dy > 0;
                case 90: // Вправо
                    return Math.Abs(dy) < Texture.Height / 2 && dx > 0;
                case 270: // Влево
                    return Math.Abs(dy) < Texture.Height / 2 && dx < 0;
                default:
                    return false;
            }
        }
        #endregion

        #endregion

        #region Выстрел
        public void Shoot(ref List<Bullet> enemyBullets, ref Canvas GameCanvas, int speed)
        {
            // Проверяем, можно ли сейчас стрелять
            if (!((DateTime.Now - lastShotTime).TotalSeconds > 3))
                return;

            // Перезарядка пушки: обновляем время последнего выстрела и блокируем стрельбу
            lastShotTime = DateTime.Now;

            double bulletX = X + Texture.Width / 2-5;
            double bulletY = Y + Texture.Height / 2-5;

            double angle = currentAngle;

            Bullet bullet = new Bullet(bulletX, bulletY, speed, angle - 90);
            enemyBullets.Add(bullet);
            GameCanvas.Children.Add(bullet.Texture);
        }
        #endregion

        #endregion
    }
}
