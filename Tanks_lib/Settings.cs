using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks_lib
{
    [Serializable]
    public class Settings
    {
        public int TankSpeed { get; set; }
        public int EnemyTankSpeed { get; set; }
        public int PlayerBulletSpeed {  get; set; }
        public int EnemyBulletSpeed {  get; set; }
        public int EnemySpawnTime { get; set; }

        public Settings() { }
    }

}
