using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Structures
{
    public class ChargingStation
    {
        public Car CarInStation { get; set; }
        public int ID { get; set; }
        public double ChargingRatePerHour { get; set; }
    }
}
