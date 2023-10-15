using One_Sgp4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tlm_v2.Models
{
    public class SatelliteInfo
    {
        public string Name;
        public string tle1;
        public string tle2;

        public double Azimuth { get => (_sphericalPoint != null) ? _sphericalPoint.y : -1;  }
        public double Elevation { get => (_sphericalPoint != null) ? _sphericalPoint.z : -1; }
        public int Range { get => (_sphericalPoint != null) ? Convert.ToInt32(_sphericalPoint.x) : -1; }

        public bool VisibleFromObserver { get => (Elevation > 0) ? true : false; }

        private Tle _tle;

        private Sgp4Data _data;
        private Point3d _sphericalPoint;

        public string GetNoradID {  get => _tle.getNoradID(); }

        public SatelliteInfo(string Name, string tle1, string tle2)
        {
            this.Name = Name;
            this.tle1 = tle1;
            this.tle2 = tle2;

            _tle = ParserTLE.parseTle(this.tle1, this.tle2);
        }

        public void UpdateSatelliteData(ObserverInfo Observer)
        {
            EpochTime currentTime = new EpochTime(DateTime.UtcNow);
            _data = SatFunctions.getSatPositionAtTime(_tle, currentTime, Sgp4.wgsConstant.WGS_84);

            _sphericalPoint = SatFunctions.calcSphericalCoordinate(Observer.Coordinate, currentTime, _data);
        }

        public List<Pass> GetFuturePasses(ObserverInfo Observer)
        {
            return One_Sgp4.SatFunctions.CalculatePasses(Observer.Coordinate, _tle, new EpochTime(DateTime.UtcNow), 15, 2, Sgp4.wgsConstant.WGS_84);
        }
    }
}
