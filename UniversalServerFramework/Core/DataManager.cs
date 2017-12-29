using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UniversalServerFramework.Core;

using System.Data.SqlClient;
using System.Text.RegularExpressions;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

//This Project created in 2017-12-29 by NextLeaves

namespace UniversalServerFramework
{
    class DataManager
    {
        private string _serverAddress = "Data Source=DESKTOP-0PM5A47;Initial Catalog=UniversalServerRepertory;Integrated Security=True";
        private static DataManager _instance;
        private SqlConnection sqlConn;
        public static DataManager Instance
        {
            get { return _instance; }
        }
        public string ServerAddress
        {
            set { _serverAddress = value; }
        }
        public DataManager()
        {
            _instance = this;
            Connected();
        }


        /// <summary>
        /// Connecting with serverAddress,and test is it ok?
        /// </summary>
        private void Connected()
        {
            sqlConn = new SqlConnection(_serverAddress);

            try
            {
                sqlConn.Open();
            }
            catch (SqlException sqlExc)
            {
                Console.WriteLine("[Connected] Method is errror;");
                Console.WriteLine("Error info : {0}.\n", sqlExc.Message);
            }
            finally
            {
                sqlConn.Close();
            }
        }

        /// <summary>
        /// 检查是否是合法字符串
        /// </summary>
        /// <param name="inputStrs">传入需要检验的字符串数组</param>
        /// <returns>非法字符（false），合法（ture）</returns>
        private bool IsSafeStr(params string[] inputStrs)
        {
            bool isSafe = false;
            foreach (string inputStr in inputStrs)
            {
                if (string.IsNullOrEmpty(inputStr)) return false;
                isSafe = !Regex.IsMatch(inputStr, @"[,|;|@|!|*|']");
                if (!isSafe) break;
            }

            return isSafe;
        }

        /// <summary>
        /// 检查是否存在用户
        /// </summary>
        /// <param name="id">用户名</param>
        /// <returns>存在（false），反之（true）</returns>
        private bool CanRegister(string id)
        {
            if (!IsSafeStr(id)) return false;

            string sql = string.Format("SELECT * FROM SubscriberTable WHERE id='{0}';", id);
            SqlCommand command = new SqlCommand(sql, sqlConn);

            try
            {
                sqlConn.Open();
                SqlDataReader dataReader = command.ExecuteReader();
                bool hasRow = dataReader.HasRows;
                dataReader.Close();
                return !hasRow;
            }
            catch (SqlException sqlExc)
            {
                Console.WriteLine("[IsSafeStr] Method is errror;");
                Console.WriteLine("Error info : {0}.\n", sqlExc.Message);
                return false;
            }
            finally
            {
                sqlConn.Close();
            }
        }

        /// <summary>
        /// 注册账号
        /// </summary>
        /// <param name="id">账户</param>
        /// <param name="password">密码</param>
        /// <returns>成功（ture），失败（false）</returns>
        public bool Register(string id, string password)
        {
            if (!CanRegister(id)) return false;

            if (!IsSafeStr(id) || !IsSafeStr(password)) return false;

            string sql = string.Format("INSERT INTO SubscriberTable(id,password) VALUES('{0}','{1}');", id, password);
            SqlCommand command = new SqlCommand(sql, sqlConn);

            try
            {
                sqlConn.Open();
                command.ExecuteNonQuery();
                return true;
            }
            catch (SqlException sqlExc)
            {
                Console.WriteLine("[Register] Method is errror;");
                Console.WriteLine("Error info : {0}.\n", sqlExc.Message);
                return false;
            }
            finally
            {
                sqlConn.Close();
            }
        }

        /// <summary>
        /// 创建角色初始数据
        /// </summary>
        /// <param name="id">用户账号</param>
        /// <returns></returns>
        public bool CreatePlayer(string id)
        {
            PlayerData playerData = new PlayerData();

            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            try
            {
                formatter.Serialize(stream, playerData);
            }
            catch (SerializationException serializeExc)
            {
                Console.WriteLine("[CreatePlayer] Method is errror;");
                Console.WriteLine("Error info : {0}.\n", serializeExc.Message);
            }

            byte[] bytesStream = stream.ToArray();

            string sql = string.Format("INSERT INTO PlayerTable(id,data) VALUES('{0}',@data);", id);
            SqlCommand command = new SqlCommand(sql, sqlConn);
            command.Parameters.Add("@data", System.Data.SqlDbType.Image);
            command.Parameters[0].Value = bytesStream;

            try
            {
                sqlConn.Open();
                command.ExecuteNonQuery();
                return true;
            }
            catch (SqlException sqlExc)
            {
                Console.WriteLine("[CreatePlayer] Method is errror;");
                Console.WriteLine("Error info : {0}.\n", sqlExc.Message);
                return false;
            }
            finally
            {
                sqlConn.Close();
            }
        }

        /// <summary>
        /// 登陆检测用户是否存在，若不存在，可以进行注册操作
        /// </summary>
        /// <param name="id">用户账号</param>
        /// <param name="password">用户密码</param>
        /// <returns></returns>
        public bool CheckLogInfomation(string id, string password)
        {
            if (!IsSafeStr(id) || !IsSafeStr(password)) return false;

            string sql = string.Format("SELECT * FORM Subscriber WHERE id='{0}',password='{1}'", id, password);
            SqlCommand command = new SqlCommand(sql, sqlConn);
            try
            {
                sqlConn.Open();
                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    return dataReader.HasRows;
                }

            }
            catch (SqlException sqlExc)
            {
                Console.WriteLine("[CheckLogInfomation] Method is errror;");
                Console.WriteLine("Error info : {0}.\n", sqlExc.Message);
                return false;
            }
            finally
            {
                sqlConn.Close();
            }
        }

        public PlayerData GetPlayerData(string id)
        {
            if (!IsSafeStr(id)) return null;

            byte[] bytes = new byte[1];
            string sql = string.Format("SELECT * FROM PlayerTable WHERE id='{0}'", id);
            SqlCommand command = new SqlCommand(sql, sqlConn);

            PlayerData tempPlayerData = new PlayerData();

            try
            {
                sqlConn.Open();
                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    if (!dataReader.HasRows) return tempPlayerData;

                    dataReader.Read();

                    long length = dataReader.GetBytes(1, 0, null, 0, 0);
                    bytes = new byte[length];
                    dataReader.GetBytes(1, 0, bytes, 0, (int)length);
                }
            }
            catch (SqlException sqlExc)
            {
                Console.WriteLine("[GetPlayerData] Method is errror;");
                Console.WriteLine("Error info : {0}.\n", sqlExc.Message);
            }
            finally
            {
                sqlConn.Close();
            }

            MemoryStream stream = new MemoryStream(bytes);
            IFormatter formatter = new BinaryFormatter();

            try
            {
                return formatter.Deserialize(stream) as PlayerData;
            }
            catch (SerializationException serializeExc)
            {
                Console.WriteLine("[GetPlayerData] Method is errror;");
                Console.WriteLine("Error info : {0}.\n", serializeExc.Message);
                return tempPlayerData;
            }
        }

        public bool SavePlayperData(Player player)
        {
            string id = player.Id;
            PlayerData tempPlayerData = player.playerData;

            string sql = string.Format("SELECT * FROM PlayerTable WHERE id='{0}';", id);
            SqlCommand command = new SqlCommand(sql, sqlConn);

            try
            {
                sqlConn.Open();
                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    if (!dataReader.HasRows) return false;
                }
            }
            catch (SqlException sqlExc)
            {
                Console.WriteLine("[SavePlayperData] Method is errror;");
                Console.WriteLine("Error info : {0}.\n", sqlExc.Message);
            }

            sql = string.Format("UPDATE PlayerTable SET data=@data WHERE id='{0}'", id);
            command = new SqlCommand(sql, sqlConn);


            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();

            try
            {
                formatter.Serialize(stream, tempPlayerData);

                command.Parameters.Add("@data", System.Data.SqlDbType.Image);
                command.Parameters[0].Value = stream.ToArray();

                command.ExecuteNonQuery();
                return true;
            }
            catch (SerializationException serializeExc)
            {
                Console.WriteLine("[SavePlayperData] Method is errror;");
                Console.WriteLine("Error info : {0}.\n", serializeExc.Message);
                return false;
            }
            catch (SqlException sqlExc)
            {
                Console.WriteLine("[SavePlayperData] Method is errror;");
                Console.WriteLine("Error info : {0}.\n", sqlExc.Message);
                return false;
            }
            finally
            {
                sqlConn.Close();
            }


        }
    }
}
