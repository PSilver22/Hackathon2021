using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Structures
{
    public class Battery
    {
        public double Capacity { get; set; }
        public double CurrentLevel { get; set; }
        public double CurrentPercentage { get => CurrentLevel / Capacity; }

        /// <summary>
        /// returns the battery level after charging for the given amount of time
        /// </summary>
        /// <param name="endTime"></param>
        /// <param name="chargeRate"></param>
        /// <returns></returns>
        public double GetNewCharge(DateTime endTime, double chargeRate){
            int chargeTime = (DateTime.Now - endTime).Minutes;

            double slope = (Capacity - CurrentLevel) / chargeRate;
            return (slope * chargeTime) + CurrentLevel;
        }
    }
}
