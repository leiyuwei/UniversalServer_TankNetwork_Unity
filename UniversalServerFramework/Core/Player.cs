using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalServerFramework.Core
{
    class Player
    {
        public string Id { get; set; }
        public PlayerData playerData
        {
            get; set;
        }        
        public PlayerTempdata playerTempdata { get; set; }
        private Conn conn;
    }
}
