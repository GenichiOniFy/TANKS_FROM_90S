using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Tanks_lib
{
    [Serializable]
    public class Settings
    {

        public int TankSpeed { get; set; }
        public int EnemyTankSpeed {get; set; }
        public int PlayerBulletSpeed { get; set; }
        public int EnemyBulletSpeed { get; set; }
        public int EnemySpawnSpeed { get; set; }

        public bool Volume { get; set; }

        public Settings()
        {
            TankSpeed = 6;
            EnemyTankSpeed = 2;
            PlayerBulletSpeed = 8;
            EnemyBulletSpeed = 6;
            EnemySpawnSpeed = 4;
            Volume = true;
        }
    }

}
