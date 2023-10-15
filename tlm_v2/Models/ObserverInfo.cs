using One_Sgp4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tlm_v2.Models
{
    public class ObserverInfo
    {
        public string Callsign { get => _name; }
        public double Latitude { get => _lat; }
        public double Longitude { get => _lon; }
        public double Height { get => _height; }
        public Coordinate Coordinate { get => _observerCoordinates; }

        private Coordinate _observerCoordinates;
        private string _name;
        private double _lat;
        private double _lon;
        private double _height; 

        public ObserverInfo(string Name, double Lat, double Lon, double Alt) 
        { 
            _name = Name;
            _lat = Lat;
            _lon = Lon;
            _height = Alt;

            _observerCoordinates = new Coordinate(Latitude, Longitude, Height);
        }
    }
}
