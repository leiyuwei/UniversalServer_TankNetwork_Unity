using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using UniversalServerFramework.Core;

namespace UniversalServerFramework
{
    class NL_Conn
    {
        private const int BUFFERSIZE = 1024;

        public byte[] Buffer { get; set; }
        public int BufferCount { get; set; }
        public Socket Socket { get; set; }
        public bool IsUse { get; set; }

        //心跳机制
        public long LastTickTime { get; set; }

        //粘包分包机制
        public byte[] LenBytes { get; set; }
        public int MsgLength { get; set; }

        public Player Player { get; set; }

        public int RemainCount
        {
            get { return BUFFERSIZE - BufferCount; }
        }

        public string GetAddress
        {
            get
            {
                if (IsUse && Socket != null) return Socket.RemoteEndPoint.ToString();
                else throw new NullReferenceException("This socket is null.");
            }
        }

        public NL_Conn()
        {
            Buffer = new byte[BUFFERSIZE];
            BufferCount = 0;
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IsUse = false;
            LastTickTime = long.MinValue;
            LenBytes = new byte[sizeof(int)];
        }

        public void Init(Socket socket)
        {
            if (IsUse) return;
            Socket = socket;
            LastTickTime = Sys.GetTimeStamp();
            IsUse = true;
        }

        public void Close()
        {
            if (!IsUse) return;
            //if (Player != null) return;
            Console.WriteLine("[Server] is disconnect {0}.", this.GetAddress);
            IsUse = false;
            Socket.Close();
            
        }


    }
}
