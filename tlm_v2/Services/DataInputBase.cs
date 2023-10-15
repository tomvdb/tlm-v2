using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tlm_v2.Services
{
    public delegate void NewDataReceived(byte[] data);

    public abstract class DataInputBase
    {
        public bool Connected { get => _connected;  }

        internal bool _connected;

        public DataInputBase() { }


    }
}
