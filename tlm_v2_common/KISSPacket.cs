namespace tlm_v2_common
{
    public class KISSPacket
    {
        private byte[] _kissData;
        private DateTime _kissTimeStamp;

        public byte[] RawData { get => _kissData; }
        public byte[] CleanData { get => StripKissData(); }

        public DateTime TimeStamp { get => _kissTimeStamp; }

        // return data with or without kiss protocol data
        public byte[] GetData(bool Clean)
        {
            if (Clean) return StripKissData();

            return RawData;
        }

        public KISSPacket(byte[] data)
        {
            _kissData = data.ToArray();
            _kissTimeStamp = DateTime.UtcNow;
        }

        public KISSPacket(byte[] data, DateTime timestamp)
        {
            _kissData = data.ToArray();
            _kissTimeStamp = timestamp;
        }

        public byte[] StripKissData()
        {
            List<byte> data = new List<byte>();

            for (int x = 2; x < _kissData.Length - 1; x++)
            {
                byte output = _kissData[x];

                if (_kissData[x] == 0xDB && _kissData[x + 1] == 0xDC)
                {
                    output = 0xC0;
                    x++;
                }
                else if (_kissData[x] == 0xDB && _kissData[x + 1] == 0xDD)
                {
                    output = 0xDB;
                    x++;
                }

                data.Add(output);
            }

            return data.ToArray();
        }

        public override string ToString()
        {
            string dt = "";

            for (int x = 0; x < _kissData.Length; x++)
            {
                dt += _kissData[x].ToString("X").PadLeft(2, '0') + " ";
            }

            return "KISS: " + dt;
        }

    }
}