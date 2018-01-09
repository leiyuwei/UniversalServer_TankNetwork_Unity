using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using static System.Text.Encoding;

using System.Data.Sql;
using System.Data.SqlClient;
using System.Timers;

using UniversalServerFramework.Core;

namespace UniversalServerFramework
{
    class NL_Server
    {
        public Socket Socket { get; set; }
        public NL_Conn[] Conns { get; set; }

        private int _maxCount;

        private SqlConnection sqlConn;

        private Timer _timer;
        private long _tickTimeSpan = 10;

        private ProtocolBase _protocolBase;

        public NL_Server()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _maxCount = 50;
            Conns = new NL_Conn[_maxCount];
            for (int current = 0; current < _maxCount; current++)
            {
                Conns[current] = new NL_Conn();
            }
            _timer = new Timer(1000);
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private int GetIndex()
        {
            for (int index = 0; index < _maxCount; index++)
            {
                if (Conns[index] == null)
                {
                    Conns[index] = new NL_Conn();
                    return index;
                }
                if (Conns[index].IsUse) continue;
                return index;
            }
            Console.WriteLine("[Server] count is full.");
            return -1;
        }

        public void Startup(string ipAddress, int port = 1234)
        {

            //SqlServerConnected();

            IPAddress tempAddress;
            IPAddress.TryParse(ipAddress, out tempAddress);
            IPEndPoint ipEP = new IPEndPoint(tempAddress, port);

            Socket.Bind(ipEP);
            Socket.Listen(_maxCount);
            Console.WriteLine("[Server] is listening···");

            _timer.Elapsed += HeartBeat;
            _timer.Start();
            Socket.BeginAccept(AcceptCb, null);
        }

        private void AcceptCb(IAsyncResult ar)
        {
            int index = GetIndex();
            if (index < 0) return;
            try
            {
                Socket tempSocket = Socket.EndAccept(ar);
                NL_Conn tempConn = Conns[index];
                tempConn.Init(tempSocket);
                Console.WriteLine("[Server] accept a client({0})", tempConn.GetAddress);
                tempConn.Socket.BeginReceive(tempConn.Buffer, tempConn.BufferCount, tempConn.RemainCount, SocketFlags.None, RecieveCb, tempConn);
                Socket.BeginAccept(AcceptCb, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("AcceptCb is error.{0}", ex.Message);
            }
        }

        private void RecieveCb(IAsyncResult ar)
        {
            NL_Conn tempConn = ar.AsyncState as NL_Conn;
            try
            {
                int count = tempConn.Socket.EndReceive(ar);
                //if (count <= 0)
                //{
                //    Console.WriteLine($"Disconnected with {tempConn.GetAddress}.");
                //    tempConn.Close();
                //}

                tempConn.BufferCount += count;
                ProcessData(tempConn);
                //string message = System.Text.Encoding.UTF8.GetString(tempConn.Buffer, tempConn.BufferCount, count);
                //Console.WriteLine("Recieve Client[{0}]'s Message: {1}", tempConn.GetAddress, message);
                //tempConn.BufferCount = count;

                // message = MessageHandle(message, tempConn);

                //SendMessage(message);
                tempConn.Socket.BeginReceive(tempConn.Buffer, tempConn.BufferCount, tempConn.RemainCount, SocketFlags.None, RecieveCb, tempConn);
            }
            catch (Exception ex)
            {
                Console.WriteLine("RecieveCb is error.{0}", ex.Message);
            }
        }

        public void ProcessData(NL_Conn conn)
        {
            if (conn.BufferCount < sizeof(Int32)) return;
            Array.Copy(conn.Buffer, conn.LenBytes, sizeof(int));
            conn.MsgLength = BitConverter.ToInt32(conn.LenBytes, 0);
            if (conn.BufferCount < (conn.MsgLength + sizeof(int))) return;
            //处理消息
            //string message = System.Text.Encoding.UTF8.GetString(conn.Buffer, sizeof(Int32), conn.MsgLength);
            //Console.WriteLine("Recieve Client[{0}]'s Message: {1}", conn.GetAddress, message);
            //SendMessage(message);

            ProtocolBase tempBase = _protocolBase.Decode(conn.Buffer, sizeof(int), conn.MsgLength);
            HandleMessage(conn, tempBase);
            //清楚已处理的消息
            conn.BufferCount -= (sizeof(Int32) + conn.MsgLength);
            Array.Copy(conn.Buffer, sizeof(Int32) + conn.MsgLength, conn.Buffer, 0, conn.BufferCount);

            if (conn.BufferCount > 0) ProcessData(conn);

        }

        public void HandleMessage(NL_Conn conn, ProtocolBase protocol)
        {
            string name = protocol.GetProtocolName;
            Console.WriteLine("Recieved ProtocolName is : {0}", name);
            if (name == "HeartBeat")
            {
                Console.WriteLine("Refresh {0} 's heartbeat.", conn.GetAddress);
                conn.LastTickTime = Sys.GetTimeStamp();
            }
        }

        public void SendData(NL_Conn conn, string message)
        {
            byte[] msgBytes = Encoding.UTF8.GetBytes(message);
            byte[] lenBytes = BitConverter.GetBytes(msgBytes.Length);
            byte[] sendBytes = lenBytes.Concat(msgBytes).ToArray();

            try
            {
                conn.Socket.BeginSend(sendBytes, 0, sendBytes.Length, SocketFlags.None, null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SendData is error.{0}", ex.Message);
            }
        }

        private void SendMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (message == "_GET") return;
            try
            {

                foreach (NL_Conn conn in Conns)
                {
                    if (conn == null) continue;
                    if (!conn.IsUse) continue;
                    byte[] bytes = Default.GetBytes(message);
                    conn.Socket.Send(bytes);
                    Console.WriteLine("[Server] transmit message:{0}", message);
                    Console.WriteLine("[Client]{0} transmited.", conn.GetAddress);
                }

                Console.WriteLine("[Server] transmit ok.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("SendMessage is error.{0}", ex.Message);

            }
        }

        private void SqlServerConnected()
        {
            string str = "Data Source=DESKTOP-0PM5A47;Initial Catalog=MessageBoard;Integrated Security=True";
            sqlConn = new SqlConnection(str);

            try
            {
                sqlConn.Open();
                Console.WriteLine("sql open successful.");
            }
            catch (Exception ex)
            {

                Console.WriteLine("SqlServerConnected is error.{0}", ex.Message);
            }
            finally
            {
                sqlConn.Close();
            }
        }

        private string MessageHandle(string message, NL_Conn conn)
        {
            //if (string.IsNullOrEmpty(message)) return;
            if (message == "_GET")
            {
                string str;
                str = OutputMessageBoard();
                return str;
            }
            else
            {

                SaveMessage(message, conn.GetAddress);
                return "";
            }

        }

        private string OutputMessageBoard()
        {
            string str = "";
            string sqlStr = string.Format("SELECT * FROM Board ;");
            SqlCommand command = new SqlCommand(sqlStr, sqlConn);
            try
            {
                sqlConn.Open();
                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    str += dataReader["ip"] + ":" + dataReader["message"] + "\n";
                }
                dataReader.Close();
                return str;

            }
            catch (Exception ex)
            {
                Console.WriteLine("OutputMessageBoard is error.{0}", ex.Message);
                return str;
            }
            finally
            {
                sqlConn.Close();
            }
        }

        private void SaveMessage(string message, string ipAddress)
        {
            string sqlStr = string.Format("INSERT INTO Board(ip,message) VALUES('{0}','{1}');", ipAddress, message);
            SqlCommand command = new SqlCommand(sqlStr, sqlConn);
            try
            {
                sqlConn.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("SaveMessage is error.{0}", ex.Message);
            }
            finally
            {
                sqlConn.Close();
            }
        }

        private void HeartBeat(object sender, ElapsedEventArgs args)
        {
            Console.WriteLine("Main Heart beat is working.");
            long nowTickTime = Sys.GetTimeStamp();
            foreach (var conn in Conns)
            {
                if (conn == null) continue;
                if (!conn.IsUse) continue;
                if (conn.LastTickTime < nowTickTime - _tickTimeSpan)
                {
                    lock (conn)
                    {
                        Console.WriteLine($"Client [{conn.GetAddress}] is disconnected.TimeStamp is more than {_tickTimeSpan} seconds.");
                        conn.Close();
                    }
                }

            }
        }
    }
}
