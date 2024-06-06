using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks_lib
{
    public class MazeGenerator // Генератор лабиринта
    {
        #region Поля
        public int Width; // Ширина Лабиринта
        public int Height; //Высота лабиринта
        private bool[,] visited; // Массив для отслеживания посещенных клеток
        private bool[,] walls; // Массив стен
        private Random random;
        #endregion

        #region Конструктор генератор лабиринта
        public MazeGenerator(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            visited = new bool[width, height];
            walls = new bool[width, height];
            random = new Random();
        }
        #endregion

        #region Методы для генерации лабиринта
        public void GenerateMaze()
        {
            #region Инициализация массивов посещенных клеток и стен
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    visited[x, y] = false; // Все клетки помечены как непосещенные
                    walls[x, y] = true; // Изначально все клетки окружены стенами
                }
            }
            walls[1, 1] = false; // Убираем стену в начальной клетке
            #endregion

            GenerateMazeRecursive(random.Next(Width), random.Next(Height));
        }

        private void GenerateMazeRecursive(int x, int y)
        {
            visited[x, y] = true; // Помечаем текущую клетку как посещенную

            int[] directions = { 0, 1, 2, 3 };
            directions = directions.OrderBy(a => random.Next()).ToArray(); // Перемешиваем массив направлений случайным образом

            #region Проходим по всем направлениям
            foreach (int direction in directions)
            {
                int nx = x, ny = y;
                #region Определяем новые координаты в зависимости от направления
                switch (direction)
                {
                    case 0: nx -= 1; break; // Влево
                    case 1: ny -= 1; break; // Вверх
                    case 2: nx += 1; break; // Вправо
                    case 3: ny += 1; break; // Вниз
                }
                #endregion

                #region Проверяем, что новые координаты в пределах лабиринта и клетка не посещена
                if (nx >= 0 && ny >= 0 && nx < Width && ny < Height && !visited[nx, ny])
                {
                    walls[(x + nx) / 2, (y + ny) / 2] = false; // "Убираем" стену между текущей и следующей клеткой
                    GenerateMazeRecursive(nx, ny);
                }
                #endregion
            }
            #endregion
        }

        #region Метод проверки, является ли клетка стеной
        public bool IsWall(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < Width && y < Height)
                return walls[x, y];
            return true; // Возвращаем true для границ лабиринта
        }
        #endregion

        #endregion
    }
}
