using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSimpleTcp;

namespace tlm_v2.Services
{
    public class DataInputKiss : DataInputBase
    {
        private SimpleTcpClient? kissTcpClient;

        private string _host = "127.0.0.1";
        private int _port = 8001;

        public event NewDataReceived? OnDataReceived;


        public DataInputKiss(string Host, int Port) 
        {
            _host = Host;
            _port = Port;

            kissTcpClient = new SimpleTcpClient(_host, _port);

            kissTcpClient.Events.Connected += Events_Connected;
            kissTcpClient.Events.Disconnected += Events_Disconnected; 
            kissTcpClient.Events.DataReceived += Events_DataReceived; 

            try
            {
                kissTcpClient.Connect();
            }
            catch (Exception Ex)
            {
                this._connected = false;
            }

        }

        public void Connect()
        {
            kissTcpClient.Connect();
        }

        private void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            OnDataReceived?.Invoke(e.Data.ToArray());
        }

        private void Events_Disconnected(object sender, ConnectionEventArgs e)
        {
            this._connected = false;
        }

        private void Events_Connected(object sender, ConnectionEventArgs e)
        {
            this._connected = true;
        }
    }
}
