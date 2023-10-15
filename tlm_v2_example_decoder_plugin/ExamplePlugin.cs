using System.Collections;
using tlm_v2_common;
using tlmv2_decoder_plugin_interface;

namespace tlm_v2_example_decoder_plugin
{
    public class ExamplePlugin : IDecoderPlugin
    {
        public int NoradID { get => 123; }

        public int Version { get => 2;  }

        public string Name { get => "Example Plugin"; }

        public string Author { get => "Tom Van den Bon - ZR6TG"; }

        public string Description { get => "Example Plugin Description"; }

        public event NewDecodedData? OnNewDecodedData;

        private int _id = 0;

        void IDecoderPlugin.parseData(KISSPacket data)
        {
            Random tempRandom = new Random();

            var newPacket = new Hashtable();
            newPacket.Add("timestamp", DateTime.Now);
            newPacket.Add("bootcount", tempRandom.Next(0, 10));
            newPacket.Add("voltage", tempRandom.NextDouble());
            newPacket.Add("voltage1", tempRandom.NextDouble());
            newPacket.Add("voltage2", tempRandom.NextDouble());
            newPacket.Add("voltage3", tempRandom.NextDouble());
            newPacket.Add("voltage4", tempRandom.NextDouble());
            newPacket.Add("voltage5", tempRandom.NextDouble());
            newPacket.Add("voltage6", tempRandom.NextDouble());
            newPacket.Add("voltage7", tempRandom.NextDouble());
            newPacket.Add("voltage8", tempRandom.NextDouble());
            newPacket.Add("voltage9", tempRandom.NextDouble());
            newPacket.Add("voltage10", tempRandom.NextDouble());
            newPacket.Add("voltage11", tempRandom.NextDouble());
            newPacket.Add("voltage12", tempRandom.NextDouble());
            newPacket.Add("voltage13", tempRandom.NextDouble());
            newPacket.Add("voltage14", tempRandom.NextDouble());
            newPacket.Add("voltage15", tempRandom.NextDouble());

            OnNewDecodedData?.Invoke(0, 0, _id++, newPacket);
        }
    }
}