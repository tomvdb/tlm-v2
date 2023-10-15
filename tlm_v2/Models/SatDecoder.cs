using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using tlm_v2_common;
using tlmv2_decoder_plugin_interface;

namespace tlm_v2.Models
{
    public class SatDecoder
    {
        private string _decoderName;
        private string[] _compatibleNoradId;

        private List<DecodedData> _decodedData;

        private List<DecodedDataGroup> _dataGroups;

        public string Name { get => _decoderName; }
        public string[] CompatibleNoradIds { get => _compatibleNoradId; }

        public List<DecodedData> DecodedData { get => _decodedData; }
        public List<DecodedDataGroup> DataGroups { get => _dataGroups; }


        // temporary for testing
        int tempDataCounter = 0;
        Random tempRandom = new Random();

        private IDecoderPlugin _plugin;


        public SatDecoder(IDecoderPlugin Plugin) 
        { 
            _decoderName = Plugin.Name;
            _compatibleNoradId = new string[] { Plugin.NoradID.ToString() };

            _dataGroups = new List<DecodedDataGroup>();
            _decodedData = new List<DecodedData>();

            _plugin = Plugin;
            _plugin.OnNewDecodedData += _plugin_OnNewDecodedData;

            // TODO: get plugin groups
            _dataGroups.Add(new DecodedDataGroup(0, "Telemetry", 0));

        }

        private void _plugin_OnNewDecodedData(int Type, int Group, int Id, Hashtable Data)
        {
            _decodedData.Add(new DecodedData(Type, Group, Id, Data));
        }

        public void SubmitKissFrame(KISSPacket packet)
        {
            _plugin.parseData(packet);
        }

        public bool Activate()
        {
            _decodedData.Clear();

            return true;
        }

        public bool Deactivate() 
        { 
            return true; 
        }

    }
}
