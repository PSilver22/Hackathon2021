using System;
using System.Collections.Generic;
using Data_Structures;

namespace Scheduler_ns
{
    public class Scheduler
    {
        public static List<Car> CarsToChargeNow(List<Car> cars, List<ChargingStation> stations, DateTime EndTime)
        {
            int numOfStations = stations.Count;

            cars.Sort((Car x, Car y) => (int)(x.ItsBattery.CurrentLevel - y.ItsBattery.CurrentLevel));

            List<Car> bottomHalf = GetLowestBatterylevelCars(cars, numOfStations);

            return bottomHalf;
        }

        public static List<Car> GetLowestBatterylevelCars(List<Car> cars, int sizeOfList)
        {
            List<Car> lowerHalf = new List<Car>(cars);
            lowerHalf.RemoveRange(sizeOfList + 1, cars.Count - sizeOfList);

            return lowerHalf;
        }

        public static List<Car> GetUpperHalfCars(List<Car> cars, int sizeOfLowerHalf)
        {
            List<Car> upperRange = new List<Car>(cars);
            upperRange.RemoveRange(0, sizeOfLowerHalf);

            return upperRange;
        }

        public static double GetAverageBatteryPercentage(List<Car> cars)
        {
            double average = 0;
            foreach (Car car in cars)
            {
                average += car.ItsBattery.CurrentLevel / car.ItsBattery.Capacity;
            }
            average /= cars.Count;

            return average;
        }
    }
}


