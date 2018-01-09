using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalServerFramework.Core
{
    class ProtocolStr : ProtocolBase
    {
        public override ProtocolBase Decode(byte[] source, int index, int count)
        {
            ProtocolStr protocolStr = new ProtocolStr();
            protocolStr._information = System.Text.Encoding.UTF8.GetString(source, index, count);
            this._protocolName = this._information.Split(',')[0];
            return protocolStr;
        }

        public override byte[] Encode() => System.Text.Encoding.Default.GetBytes(this._information);

        public override string PrintProtocolInformation() => _information;

    }
}
