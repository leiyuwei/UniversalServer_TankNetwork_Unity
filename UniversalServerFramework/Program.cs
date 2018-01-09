
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UniversalServerFramework.Core;

namespace UniversalServerFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            //string id = "wp81023729504";
            //string password = "123456";

            //DataManager dataManager = new DataManager();

            //bool isOK = dataManager.Register(id, password);
            //if (isOK) Console.WriteLine("Register is OK.");
            //else Console.WriteLine("Register is false");

            //isOK = dataManager.CreatePlayer(id);
            //if (isOK) Console.WriteLine("CreatePlayer is OK.");
            //else Console.WriteLine("CreatePlayer is false");

            //PlayerData playerData = dataManager.GetPlayerData(id);
            //if (playerData != null) Console.WriteLine("Player data Score : " + playerData.Score);
            //else Console.WriteLine("Error in PlayerData.");

            //playerData.Score -= 10;
            //Player player = new Player();
            //player.Id = id;
            //player.playerData = playerData;
            //dataManager.SavePlayperData(player);

            //playerData = dataManager.GetPlayerData(id);
            //if (playerData != null) Console.WriteLine("Player data Score : " + playerData.Score);
            //else Console.WriteLine("Error in PlayerData.");


            //测试粘包分包处理

            //NL_Server server = new NL_Server();
            //server.Startup("127.0.0.1");            


            Console.Read();
        }
    }
}
