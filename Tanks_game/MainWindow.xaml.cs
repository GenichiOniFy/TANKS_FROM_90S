using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.Windows.Threading;
using System.Xml.Serialization;
using Tanks_lib;
using static System.Formats.Asn1.AsnWriter;
using System.Media;
using System.Windows.Media.Media3D;

namespace Tanks_game
{
    public partial class MainWindow : Window
    {
        #region Поля и свойства

        #region Скорборд
        //С обычным словарём не работало, поэтому пришлось гуглить и использовать вот эту волшебную коллекцию
        public ObservableCollection<KeyValuePair<string, int>> Scores { get; set; }
        #endregion

        #region Музыка
        private MediaPlayer GameSound;
        private TimeSpan GameSoundTime;
        private MediaPlayer ExplosionSound;
        private MediaPlayer ScoreBoardSound;
        #endregion

        #region Состояния движения
        private bool isMovingUp;
        private bool isMovingDown;
        private bool isMovingLeft;
        private bool isMovingRight;
        #endregion

        #region Списки пуль, вражеских пуль, врагов, стен
        private List<Bullet> bullets;
        public List<Wall> walls;
        public List<Bullet> enemyBullets;
        private List<Enemy> enemies;
        #endregion

        #region Таймеры
        public DispatcherTimer gameTimer;
        public DispatcherTimer enemySpawnTimer;
        #endregion

        #region Поле "ТАНК ИГРОКА"
        private Player tank;
        #endregion

        #endregion

        #region Конструктор
        public MainWindow()
        {
            InitializeComponent();

            #region Загрузка звука на фоне
            GameSoundTime = new TimeSpan(0, 0, 40);
            GameSound = new MediaPlayer();
            GameSound.Open(new Uri("Sounds/GameSound.wav", System.UriKind.Relative));
            GameSound.Volume = 0.3; //Громкость музыки на фоне.
            if (MainMenu.set!.Volume)
            {
                GameSound.Play();
            }
            else
            {
                var Volume_image = (Image)VolumeButton.Content;
                Volume_image.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Button/Volume_off.png"));
            }
            
            #endregion

            #region Скорборд
            Scores = new ObservableCollection<KeyValuePair<string, int>>();
            ScoreList.ItemsSource = Scores;
            #endregion

            InitializeGame();
        }
        #endregion

        #region Методы

        #region Метод цикла музыки на фоне
        private void CheckSoundEnd()
        {
            if (GameSound.Position >= GameSoundTime)
            {
                GameSound.Stop();
                GameSound.Position = new TimeSpan(0, 0, 1);
                GameSound.Play();
            }
        }
        #endregion

        #region Инициализация игры
        private void InitializeGame()
        {
            #region Реализация таймера для обновления кадров игры
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(2);//Каждые 2 миллисекунды обновляется кадр
            gameTimer.Tick += GameTick!;
            #endregion

            #region Реализыция таймера для создания противников
            enemySpawnTimer = new DispatcherTimer();
            enemySpawnTimer.Interval = TimeSpan.FromSeconds(MainMenu.set!.EnemySpawnSpeed);
            enemySpawnTimer.Tick += CreateEnemy!;
            #endregion

            #region Создание списков пуль, стен, врагов
            bullets = new List<Bullet>();
            walls = new List<Wall>();
            enemies = new List<Enemy>();
            enemyBullets = new List<Bullet>();
            #endregion

            #region Создание танка игрока и добавление его на канву
            tank = new Player(GameCanvas, MainMenu.set!.TankSpeed, walls);;
            Canvas.SetLeft(tank.Texture, 100);
            Canvas.SetTop(tank.Texture, 100);
            GameCanvas.Children.Add(tank.Texture);
            #endregion
        }
        #endregion

        #region Выполнение методов и при создании канвы
        private void GameCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            CreateRandomWalls();
            CreateBorder();
            UpdateName(MainMenu.Name!);
            if (MainMenu.ScoreBoard!.Count > 0)
            {
                UpdateHighScore(MainMenu.ScoreBoard!.Values.Max());
            }
            else
            {
                UpdateHighScore(0);
            }
        }
        #endregion

        #region Создание лабирина из стен
        private void CreateRandomWalls()
        {
            MazeGenerator maze = new MazeGenerator((int)GameCanvas.ActualWidth / 90, (int)GameCanvas.ActualHeight / 90); // Создаем лабиринт
            maze.GenerateMaze(); // Генерируем лабиринт
            DrawMaze(maze); // Рисуем лабиринт на канве
        }
        private void DrawMaze(MazeGenerator maze)
        {
            double cellSize = 30; // Размер клетки
            for (int x = 0; x < maze.Width; x++)
            {
                for (int y = 0; y < maze.Height; y++)
                {
                    if (maze.IsWall(x, y))
                    {
                        int[][] perm = new int[][] { new int[] { 0, 0 }, new int[] { 30, 0}, new int[] { 60, 0 }, new int[] { 0, 30 }, new int[] { 30, 30 }, new int[] { 60, 30 }, new int[] { 0, 60 }, new int[] { 30, 60 }, new int[] { 60, 60 } };
                        for (int i=0; i < 9; i++)
                        {
                            Wall wall = new Wall(x * 90 + perm[i][0], y * 90 + perm[i][1], cellSize, cellSize);
                            walls.Add(wall);
                            GameCanvas.Children.Add(wall.Texture);
                        }
                        
                    }
                }
            }
        }
        #endregion

        #region Создание рамки вокруг игрового поля
        private void CreateBorder()
        {
            double canvasWidth = GameCanvas.ActualWidth;
            double canvasHeight = GameCanvas.ActualHeight;

            int numTilesHorizontal = (int)Math.Ceiling(canvasWidth / 90); // Количество плиток по горизонтали
            int numTilesVertical = (int)Math.Ceiling(canvasHeight / 90); // Количество плиток по вертикали

            BitmapImage s = new BitmapImage(new Uri("pack://application:,,,/Resources/border.jpg"));
            // Верхняя и нижняя границы
            for (int i = 0; i < numTilesHorizontal; i++)
            {
                Image topBorder = new Image { Width = 90, Height = 90, Source = s };
                Canvas.SetLeft(topBorder, i * 90);
                Canvas.SetTop(topBorder, 0);
                GameCanvas.Children.Add(topBorder);
                Image bottomBorder = new Image { Width = 90, Height = 90, Source = s };
                Canvas.SetLeft(bottomBorder, i * 90);
                Canvas.SetTop(bottomBorder, canvasHeight - 90);
                GameCanvas.Children.Add(bottomBorder);
            }

            // Левая и Правая границы
            for (int i = 1; i < numTilesVertical - 1; i++) // Избегаем повторного создания угловых плиток
            {
                Image leftBorder = new Image { Width = 90, Height = 90, Source = s };
                Canvas.SetLeft(leftBorder, 0);
                Canvas.SetTop(leftBorder, i * 90);
                GameCanvas.Children.Add(leftBorder);
                Image rightBorder = new Image { Width = 90, Height = 90, Source = s };
                Canvas.SetLeft(rightBorder, canvasWidth - 90);
                Canvas.SetTop(rightBorder, i * 90);
                GameCanvas.Children.Add(rightBorder);
            }
        }
        #endregion

        #region Отображение кадра
        private void GameTick(object sender, EventArgs e)
        {
            tank.MoveTank(isMovingUp, isMovingDown, isMovingLeft, isMovingRight);
            MoveBullets();
            MoveEnemiesTowardsPlayer();
            MoveEnemyBullets();
            CheckSoundEnd();
        }
        #endregion

        #region Кнопка старта
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            GameSound.Play();
            gameTimer.Start();
            enemySpawnTimer.Start();
        }
        #endregion

        #region Кнопка рестарта
        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            gameTimer.Stop();
            enemySpawnTimer.Stop();
            GameCanvas.Children.Clear();
            InitializeGame();
            CreateRandomWalls();
            CreateBorder();
            UpdateScore(tank.Score);
            gameTimer.Start();
            enemySpawnTimer.Start();
        }
        #endregion

        #region Кнопка стоп
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            GameSound.Stop();
            gameTimer.Stop();
            enemySpawnTimer.Stop();
        }
        #endregion

        #region Кнопка Управления звуком
        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            var Volume_image = (Image)VolumeButton.Content;
            if (MainMenu.set!.Volume)
            {
                GameSound.Pause();
                MainMenu.set!.Volume = false;
                Volume_image.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Button/Volume_off.png"));
            }
            else
            {
                GameSound.Play();
                MainMenu.set!.Volume = true;
                Volume_image.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Button/Volume_on.png"));
            }
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Settings));
            using (Stream fStream = new FileStream((string)Application.Current.Resources["Settings"] as string, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                xmlSerializer.Serialize(fStream, MainMenu.set);
            }
        }
        #endregion

        #region Кнопка вернуться в меню
        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            GameSound.Close();
            ScoreBoardSound?.Stop();
            gameTimer.Stop();
            enemySpawnTimer.Stop();
            this.Close();

        }
        #endregion

        #region Создание противника
        private void CreateEnemy(object sender, EventArgs e)
        {
            double dist = Math.Sqrt(Math.Pow(GameCanvas.ActualWidth, 2) + Math.Pow(GameCanvas.ActualHeight, 2)) / 3;
            Random rand = new Random();
            double x = rand.Next((int)GameCanvas.ActualWidth);
            double y = rand.Next((int)GameCanvas.ActualHeight);

            // Проверка места создания врага
            while (tank.CheckWallCollision(x, y, 70, 70) || Math.Sqrt(Math.Pow(x-Canvas.GetLeft(tank.Texture),2) + Math.Pow(y - Canvas.GetTop(tank.Texture), 2))<dist)
            {
                x = rand.Next((int)GameCanvas.ActualWidth);
                y = rand.Next((int)GameCanvas.ActualHeight);
            }

            Enemy enemy = new Enemy(x, y, MainMenu.set!.EnemyTankSpeed,GameCanvas.ActualWidth,GameCanvas.ActualHeight);
            Canvas.SetLeft(enemy.Texture, x);
            Canvas.SetTop(enemy.Texture, y);
            GameCanvas.Children.Add(enemy.Texture);
            enemies.Add(enemy);
        }
        #endregion

        #region Отображение движения противников
        private void MoveEnemiesTowardsPlayer()
        {
            foreach (var enemy in enemies)
            {
                enemy.MoveTowardsPlayer(Canvas.GetLeft(tank.Texture) + tank.Texture.Width / 2-35, Canvas.GetTop(tank.Texture) + tank.Texture.Height / 2-35,walls);
                Canvas.SetLeft(enemy.Texture, enemy.X);
                Canvas.SetTop(enemy.Texture, enemy.Y);

                double playerX = Canvas.GetLeft(tank.Texture);
                double playerY = Canvas.GetTop(tank.Texture);

                // Проверяем, смотрит ли враг на игрока и находится ли на одной линии с ним
                if (enemy.IsFacingPlayer(playerX, playerY))
                {
                    enemy.Shoot(ref enemyBullets, ref GameCanvas, MainMenu.set!.EnemyBulletSpeed);
                }
            }
        }
        #endregion

        #region Отображение движения снарядов игрока
        private void MoveBullets()
        {
            #region Создание временных списков для хранения пуль и изображений взрывов, которые нужно удалить
            List<Bullet> bulletsToRemove = new List<Bullet>();
            List<Image> explosionsToRemove = new List<Image>();
            #endregion

            foreach (var bullet in bullets)
            {
                #region Проверка столкновения с рамкой
                double borderWidth = 90; 
                if (bullet.X < borderWidth || bullet.X+10 > GameCanvas.ActualWidth - borderWidth ||
                    bullet.Y < borderWidth || bullet.Y+10 > GameCanvas.ActualHeight - borderWidth)
                {
                    bulletsToRemove.Add(bullet);
                    continue; // Столкновение с рамкой
                }
                #endregion

                #region Проверка столкновения пули со стеной
                List<Wall> _walls = bullet.CheckCollisionWithWall(walls);
                if (_walls!=null)
                {
                    bulletsToRemove.Add(bullet);
                    foreach(Wall w in _walls)
                    {
                        if (w.life)
                        {
                            double x = Canvas.GetLeft(w.Texture);
                            double y = Canvas.GetTop(w.Texture);
                            GameCanvas.Children.Remove(w.Texture);
                            w.Break();
                            Canvas.SetLeft(w.Texture, x);
                            Canvas.SetTop(w.Texture, y);
                            GameCanvas.Children.Add(w.Texture);
                        }
                        else
                        {
                            GameCanvas.Children.Remove(w.Texture);
                            walls.Remove(w);
                        }
                        
                    }
                    continue; // Пропускаем перемещение пули, если она столкнулась со стеной
                }
                #endregion

                bullet.Move();
                Canvas.SetLeft(bullet.Texture, bullet.X);
                Canvas.SetTop(bullet.Texture, bullet.Y);

                // Проверяем столкновение с врагами
                foreach (var enemy in enemies)
                {
                    if (CheckCollision(bullet, enemy))
                    {
                        #region Загрузка звука взрыва
                        if (MainMenu.set!.Volume)
                        {
                            ExplosionSound = new MediaPlayer();
                            ExplosionSound.Open(new Uri("Sounds/ExplosionSound.wav", System.UriKind.Relative));
                            ExplosionSound.Play();
                        }

                        #endregion

                        #region Создаем изображение взрыва
                        Image explosion = new Image
                        {
                            Width = 100,
                            Height = 100,
                            Source = new BitmapImage(new Uri("pack://application:,,,/Resources/fire.png")), 
                            Stretch = Stretch.Fill
                        };
                        #endregion

                        #region Обновление очков
                        tank.Score += (50/MainMenu.set!.TankSpeed+ 100/MainMenu.set!.PlayerBulletSpeed + 100/MainMenu.set!.EnemySpawnSpeed) + (10*MainMenu.set!.EnemyTankSpeed+5*MainMenu.set!.EnemyBulletSpeed);
                        UpdateScore(tank.Score);
                        if (tank.Score > int.Parse(HighScore.Text.Split()[2]))
                        {
                            UpdateHighScore(tank.Score);
                        }
                        #endregion

                        #region Добавляем взрыв на канву
                        Canvas.SetLeft(explosion, Canvas.GetLeft(enemy.Texture));
                        Canvas.SetTop(explosion, Canvas.GetTop(enemy.Texture));
                        GameCanvas.Children.Add(explosion);
                        #endregion

                        #region Чистим списки
                        // Добавляем изображение взрыва в список на удаление
                        explosionsToRemove.Add(explosion);

                        // Добавляем пулю в список на удаление
                        bulletsToRemove.Add(bullet);
                        // Удаляем врага
                        GameCanvas.Children.Remove(enemy.Texture);
                        enemies.Remove(enemy);
                        #endregion

                        break; // Выходим из внутреннего цикла, чтобы не проверять столкновение с другими врагами
                    }
                }

            }

            #region Удаляем пули и взрывы
            // Удаляем пули, которые столкнулись с врагами
            foreach (var bulletToRemove in bulletsToRemove)
            {
                GameCanvas.Children.Remove(bulletToRemove.Texture);
                bullets.Remove(bulletToRemove);
            }

            // Удаляем изображения взрывов после определенного времени
            foreach (var explosionToRemove in explosionsToRemove)
            {
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += (sender, e) =>
                {
                    GameCanvas.Children.Remove(explosionToRemove);
                    timer.Stop();
                };
                timer.Start();
            }
            #endregion

            #region Удаление снарядов, которые вышли за пределы канвы, почему-то не всегда работает
            //X< -10 это на всякий случай, а то когда < 0, то не всегда работает
            bullets.RemoveAll(b => b.X <-10 || b.X > GameCanvas.ActualWidth || b.Y < 0 || b.Y > GameCanvas.ActualHeight);
            #endregion
        }

        #region Проверка столкновения пули с врагом
        private bool CheckCollision(Bullet bullet, Enemy enemy)
        {
            Rect bulletRect = new Rect(bullet.X, bullet.Y, bullet.Texture.Width, bullet.Texture.Height);
            Rect enemyRect = new Rect(Canvas.GetLeft(enemy.Texture), Canvas.GetTop(enemy.Texture), enemy.Texture.Width, enemy.Texture.Height);
            return bulletRect.IntersectsWith(enemyRect);
        }
        #endregion

        #endregion

        #region Обновление имени игрока
        private void UpdateName(string name)
        {
            PlayerName.Text = $"PLAYER NAME: {name}";
        }
        #endregion

        #region Обновление наивысшего скора
        private void UpdateHighScore(int score)
        {
            HighScore.Text = $"HIGH SCORE: {score}";
        }
        #endregion

        #region Обновление очков
        private void UpdateScore(int newScore)
        {
            PlayerScore.Text = $"SCORE: {newScore}";
        }
        #endregion

        #region Отображение движения снарядов противника
        private void MoveEnemyBullets()
        {
            List<Bullet> bulletsToRemove = new List<Bullet>();
            foreach (var bullet in enemyBullets)
            {
                #region Проверка на столкновение пули с рамкоц
                double borderWidth = 90;

                if (bullet.X < borderWidth || bullet.X + 10 > GameCanvas.ActualWidth - borderWidth ||
                    bullet.Y < borderWidth || bullet.Y + 10 > GameCanvas.ActualHeight - borderWidth)
                {
                    bulletsToRemove.Add(bullet);
                    continue; // Столкновение с рамкой
                }
                #endregion

                #region Проверка столкновения пули со стеной
                List<Wall> _walls = bullet.CheckCollisionWithWall(walls);
                if (_walls != null)
                {
                    bulletsToRemove.Add(bullet);
                    foreach (Wall w in _walls)
                    {
                        if (w.life)
                        {
                            double x = Canvas.GetLeft(w.Texture);
                            double y = Canvas.GetTop(w.Texture);
                            GameCanvas.Children.Remove(w.Texture);
                            w.Break();
                            Canvas.SetLeft(w.Texture, x);
                            Canvas.SetTop(w.Texture, y);
                            GameCanvas.Children.Add(w.Texture);
                        }
                        else
                        {
                            GameCanvas.Children.Remove(w.Texture);
                            walls.Remove(w);
                        }

                    }
                    continue; // Пропускаем перемещение пули, если она столкнулась со стеной
                }
                #endregion

                bullet.Move();
                Canvas.SetLeft(bullet.Texture, bullet.X);
                Canvas.SetTop(bullet.Texture, bullet.Y);

                #region Пуля попала в игрока
                if (bullet.CheckCollisionWithPlayer(tank)) // Проверяем столкновение с игроком
                {
                    PlayerHit();//Игрок проиграл
                    bulletsToRemove.Add(bullet);
                }
                #endregion
            }
            #region Удаляем пульки
            bulletsToRemove.RemoveAll(b => b.X < 0 || b.X > GameCanvas.ActualWidth || b.Y < 0 || b.Y > GameCanvas.ActualHeight);

            foreach (var bullet in bulletsToRemove)
            {
                GameCanvas.Children.Remove(bullet.Texture);
                enemyBullets.Remove(bullet);
            }
            #endregion
        }
        #endregion

        #region Проигрыш
        private void PlayerHit()
        {
            #region Остановка таймеров и музыки
            gameTimer.Stop();
            enemySpawnTimer.Stop();
            GameSound.Stop();
            #endregion

            #region Добавление результата в скорборд

            if (MainMenu.ScoreBoard!.ContainsKey(MainMenu.Name!))
            {
                if (tank.Score > MainMenu.ScoreBoard[MainMenu.Name!])
                {
                    MainMenu.ScoreBoard![MainMenu.Name!] = tank.Score;
                }
            }
            else
            {
                MainMenu.ScoreBoard!.Add(MainMenu.Name!, tank.Score);
            }
            
            Scores.Clear();

            Dictionary<string, int> sortedDictionary = MainMenu.ScoreBoard.OrderByDescending(x => x.Value).Take(20).ToDictionary(x => x.Key, x => x.Value);


            foreach (var score in sortedDictionary)
            {
                Scores.Add(score);
            }

            #endregion

            #region Сериализация скорборда
            var dataContractSerializer = new DataContractSerializer(typeof(Dictionary<string, int>));
            using (Stream fStream = new FileStream((string)Application.Current.Resources["ScoreBoard"] as string, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                dataContractSerializer.WriteObject(fStream, sortedDictionary);
            }
            #endregion

            #region Загрузка звука на скорборде
            if (MainMenu.set!.Volume)
            {
                ScoreBoardSound = new MediaPlayer();
                ScoreBoardSound.Open(new Uri("Sounds/ScoreBoardSound.wav", System.UriKind.Relative));
                ScoreBoardSound.Volume = 0.3;
                ScoreBoardSound.Play();
            }
            #endregion

            #region Выводим текущие очки на скоборд
            CurScore.Text = $"YOUR SCORE: {tank.Score}";
            #endregion

            #region Отображаем скорборд
            GridGame.Visibility = Visibility.Collapsed;
            Scoreboard.Visibility = Visibility.Visible;
            #endregion

        }
        #endregion

        #region Управление
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                    isMovingUp = true;
                    break;
                case Key.S:
                    isMovingDown = true;
                    break;
                case Key.A:
                    isMovingLeft = true;
                    break;
                case Key.D:
                    isMovingRight = true;
                    break;
                case Key.L:
                    tank.Shoot(ref bullets, MainMenu.set.PlayerBulletSpeed);
                    break;
            }
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                    isMovingUp = false;
                    break;
                case Key.S:
                    isMovingDown = false;
                    break;
                case Key.A:
                    isMovingLeft = false;
                    break;
                case Key.D:
                    isMovingRight = false;
                    break;
            }
        }
        #endregion

        #endregion
    }
}