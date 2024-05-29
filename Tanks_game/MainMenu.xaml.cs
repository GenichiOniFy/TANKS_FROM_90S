using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Media;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Runtime.Serialization;
using Tanks_lib;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Tanks_game
{

    public partial class MainMenu : Window
    {

        #region Поля
        static public Settings? set;
        static public string? Name;
        static public Dictionary<string, int>? ScoreBoard;
        #endregion

        #region Конструктор
        public MainMenu()
        {
            InitializeComponent();
            #region Восстановление рекордов из файла
            try
            {
                var dataContractSerializer = new DataContractSerializer(typeof(Dictionary<string, int>));
                using (Stream fStream = new FileStream((string)Application.Current.Resources["ScoreBoard"] as string, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    ScoreBoard = (Dictionary<string, int>)dataContractSerializer.ReadObject(fStream)!;
                }
            }
            catch
            {
                ScoreBoard = new Dictionary<string, int>();
            }
            #endregion

            #region Восстановление настроек из файла
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Settings));
                using (Stream fStream = new FileStream((string)Application.Current.Resources["Settings"] as string, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    set = (Settings)xmlSerializer.Deserialize(fStream)!;
                }
            }
            catch
            {
                set = new Settings();
            }
            PlayerTankSpeedSlider.Value = set.TankSpeed;
            EnemyTankSpeedSlider.Value = set.EnemyTankSpeed;
            PlayerBulletSpeedSlider.Value = set.PlayerBulletSpeed;
            EnemyBulletSpeedSlider.Value = set.EnemyBulletSpeed;
            EnemySpawnSpeedSlider.Value = set.EnemySpawnSpeed;
            #endregion
        }
        #endregion

        #region Методы

        #region Кнопка где имя подтверждается
        private void EnterName_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox.Text != string.Empty)
            {
                Name = TextBox.Text;
                EnterNamePanel.Visibility = Visibility.Collapsed;
                MainWindow gameWindow = new MainWindow();
                gameWindow.Show();
                gameWindow.gameTimer.Start();
                gameWindow.enemySpawnTimer.Start();
            }
        }
        #endregion

        #region Чтобы имя нормальное писали
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, "^[a-zA-Z0-9_]+$"))
            {
                e.Handled = true;
            }
        }
        #endregion

        #region Кнопка "начать игру"
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            EnterNamePanel.Visibility = Visibility.Visible;
        }
        #endregion

        #region Переход на github
        void Open_github(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/GenichiOniFy/TANKS_FROM_90S") { UseShellExecute = true });
        }
        #endregion

        #region Кнопка настроек
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsPanel.Visibility = Visibility.Visible;
        }
        #endregion

        #region Кнопка применить настрйки
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            set!.TankSpeed = (int)PlayerTankSpeedSlider.Value;
            set.EnemyTankSpeed = (int)EnemyTankSpeedSlider.Value;
            set.PlayerBulletSpeed = (int)PlayerBulletSpeedSlider.Value;
            set.EnemyBulletSpeed = (int)EnemyBulletSpeedSlider.Value;
            set.EnemySpawnSpeed = (int)EnemySpawnSpeedSlider.Value;

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Settings));
            using (Stream fStream = new FileStream((string)Application.Current.Resources["Settings"] as string, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                xmlSerializer.Serialize(fStream, set);
            }

            SettingsPanel.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Кнопка выход
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion

        #endregion
    }
}
