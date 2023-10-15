using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using tlm_v2_common;

namespace tlm_v2.Services
{
    public class KISSPlayer
    {
        private List<KISSPacket> _packets;
        private int _currentPacket = -1;
        private String _fileName = "";

        public string FileName { get { return _fileName; } }
        public KISSPacket? GetNext { get
            {
                if (_currentPacket < _packets.Count-1)
                {
                    _currentPacket++;
                    return _packets[_currentPacket];
                }

                return null;
            }
        }

        public int Count { get =>  _packets.Count; }
        public int Current { get => _currentPacket + 1; }

        // load kiss file
        public KISSPlayer(string name) 
        { 
            _fileName = name;
            _packets = new List<KISSPacket>();

            byte[] fileBytes = File.ReadAllBytes(_fileName);

            int state = 0;

            List<byte>  packetData = new List<byte>();

            for(int x = 0; x < fileBytes.Length; x++)
            {
                byte b = fileBytes[x];
                switch(state)
                {
                    // read until we get start of a kiss packet - discard the rest

                    case 0:
                        
                        if (b == 0xC0)
                        {

                            // https://en.wikipedia.org/wiki/KISS_(amateur_radio_protocol)
                            // Similar to SLIP, back-to-back FEND codes should not be interpreted as empty frames. Instead, all but the last FEND code should be discarded. This can be used for synchronization, and can be used to give the receiver's AGC time to stabilize.
                            if (x < fileBytes.Length - 1)
                            {
                                if (fileBytes[x + 1] == 0xC0)
                                {
                                    continue;
                                }
                                

                            }

                            state = 1;
                            packetData.Clear();
                            packetData.Add(b);
                        }
                        break;

                    case 1:

                        
                        
                        packetData.Add(b);

                        // add data until we hit 0xC0
                        // proper kiss data won't have any 0xC0 data because it will be espaced
                        // if the last data is incomplete then it won't be added
                        if (b == 0xC0)
                        {
                            _packets.Add(new KISSPacket(packetData.ToArray()));
                            packetData.Clear();
                            state = 0;
                        }
                        break;
                }
            }
        }

    }
}
