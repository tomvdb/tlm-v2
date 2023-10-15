using System.Collections;
using tlm_v2_common;

namespace tlmv2_decoder_plugin_interface
{
    public delegate void NewDecodedData(int Type, int Group, int Id, Hashtable Data);

    public interface IDecoderPlugin
    {
        int NoradID { get; }
        int Version { get; }
        string Name { get; }
        string Author { get; }
        string Description { get; }

        public event NewDecodedData? OnNewDecodedData;

        public void parseData(KISSPacket data);
    }
}