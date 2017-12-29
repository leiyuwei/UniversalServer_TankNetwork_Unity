using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace UniversalServerFramework.Core
{
    [Serializable]
    class PlayerData
    {
        public int Score { get; set; } = 100;
    }
}
