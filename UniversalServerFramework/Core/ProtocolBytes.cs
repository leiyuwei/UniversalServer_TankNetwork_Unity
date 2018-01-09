using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalServerFramework.Core
{
    class ProtocolBytes : ProtocolBase
    {
        private byte[] bytes { get; set; }
        public override ProtocolBase Decode(byte[] source, int index, int count)
        {
            ProtocolBytes protocolBytes = new ProtocolBytes();
            protocolBytes.bytes = new byte[count];
            Array.Copy(source, index, this.bytes, 0, count);
            return protocolBytes;
        }

        public override byte[] Encode() => bytes;
        public override string PrintProtocolInformation()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(((int)bytes[i]).ToString() + " ");
            }

            return sb.ToString();
        }

        //字节流辅助协议

        //添加字符串
        public void AddString(string message)
        {
            int len = message.Length;
            byte[] lenBytes = BitConverter.GetBytes(len);
            byte[] msglength = System.Text.Encoding.UTF8.GetBytes(message);
            if (bytes == null)
                bytes = lenBytes.Concat(msglength).ToArray();
            else
                bytes = bytes.Concat(lenBytes).Concat(msglength).ToArray();
        }

        public string GetString(int start, ref int end)
        {
            string returnMessage = "";
            if (bytes == null) return returnMessage;
            if (bytes.Length < start + sizeof(int)) return returnMessage;
            int msglen = BitConverter.ToInt32(bytes, start);
            if (bytes.Length < start + sizeof(int) + msglen) return returnMessage;
            returnMessage = System.Text.Encoding.UTF8.GetString(bytes, start + sizeof(int), msglen);
            end = start + sizeof(int) + msglen;
            return returnMessage;
        }

        public string GetString(int start, int end = 0) => GetString(start);

        //添加整型数据
        public void AddNumber(int num)
        {

            byte[] lenBytes = BitConverter.GetBytes(num);
            if (bytes == null) bytes = lenBytes;
            else
                bytes = bytes.Concat(lenBytes).ToArray();
        }

        public int GetNumber(int start, ref int end)
        {
            int returnInt = 0;
            if (bytes == null) return returnInt;
            if (bytes.Length < start + sizeof(int)) return returnInt;
            returnInt = BitConverter.ToInt32(bytes, start);
            end = start + sizeof(int);
            return returnInt;
        }

        public int GetInt(int start, int end = 0) => GetInt(start);

        //添加浮点数数据
        public void AddNumber(float num)
        {

            byte[] lenBytes = BitConverter.GetBytes(num);
            if (bytes == null) bytes = lenBytes;
            else
                bytes = bytes.Concat(lenBytes).ToArray();
        }

        public float GetNumber(int start, ref float end)
        {
            float returnInt = 0;
            if (bytes == null) return returnInt;
            if (bytes.Length < start + sizeof(int)) return returnInt;
            returnInt = BitConverter.ToInt64(bytes, start);
            end = start + sizeof(int);
            return returnInt;
        }

        public float GetNumber(int start, float end = 0) => GetInt(start);
    }
}
