using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalServerFramework.Core
{
    abstract class ProtocolBase
    {
        protected string _information;
        protected string _protocolName;
        public string GetProtocolName
        {
            get { return _protocolName; }
            protected set { _protocolName = value; }
        }

        public abstract ProtocolBase Decode(byte[] source, int index, int count);

        public abstract byte[] Encode();

        public virtual string PrintProtocolInformation() => _information;

    }
}
