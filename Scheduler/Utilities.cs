using System;
using System.Collections.Generic;
using System.Threading;
using Data_Structures;

namespace Utilities_ns
{
    public class Utilities
    {
        public static List<Employee> chargingEmployees = new();
        public static int numChargingStations = 3;
        public static Mutex chargingStationsMutex = new Mutex();
        public static double chargeRate = 60;
        public static double chargeGoalPercentage = 10;

        public static Mutex waitingEmployeesMutex = new Mutex();
        public static List<Employee> awaitingUpdateEmployees = new();

        public static List<Employee> employees = new();
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
            if (sizeOfList < lowerHalf.Count)
            {
                lowerHalf.RemoveRange(sizeOfList + 1, Math.Max(cars.Count, cars.Count - sizeOfList));
            }

            return lowerHalf;
        }

        public static List<Car> GetUpperHalfCars(List<Car> cars, int sizeOfLowerHalf)
        {
            List<Car> upperRange = new List<Car>(cars);
          
            upperRange.RemoveRange(0, Math.Min(cars.Count, sizeOfLowerHalf));

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

            return average * 100;
        }

        public static void UpdateBatterylevel(List<Employee> ChargingEmployees, double ChargeTimeInMinutes)
        {
            if (ChargeTimeInMinutes == 0)
            {
                return;
            }

            foreach (Employee employee in ChargingEmployees)
            {
                employee.ItsCar.ItsBattery.CurrentLevel = Math
                    .Min(employee.ItsCar.ItsBattery.CurrentLevel + (ChargeTimeInMinutes * Utilities.chargeRate / 60),
                    employee.ItsCar.ItsBattery.Capacity);
            }
        }

        public static bool WaitingForUpdate(Employee e)
        {
            if (e == null)
            {
                return false;
            }

            foreach (Employee i in Utilities.awaitingUpdateEmployees)
            {
                if (i.Name == e.Name)
                {
                    return true;
                }
            }

            return false;
        }

        public static List<Car> GetCarList(List<Employee> employeeList)
        {
            List<Car> cars = new();

            foreach (Employee employee in employeeList)
            {
                cars.Add(employee.ItsCar);
            }

            return cars;
        }

        public static double TimeToChargeInMinutes(double lowerAverage, double upperAverage)
        {
            double BatteryDifference = upperAverage - lowerAverage;
            return BatteryDifference / chargeRate / 60;
        }

        public static bool ReachedSecondStage(List<Employee> employees)
        {
            List<Car> cars = GetCarList(employees);
            List<Car> lowerCars = GetLowestBatterylevelCars(cars, numChargingStations);
            List<Car> upperCars = GetUpperHalfCars(cars, numChargingStations);

            double lowerAverage = GetAverageBatteryPercentage(lowerCars);
            double upperAverage = GetAverageBatteryPercentage(upperCars);

            double timeToChargeToUpperAverage = TimeToChargeInMinutes(lowerAverage, upperAverage);

            return timeToChargeToUpperAverage >= 120;
        }

        public static bool CarExists(int LicensePlateNumber)
        {
            foreach(Employee employee in employees)
            {
                if (employee.ItsCar.LicensePlateNumber == LicensePlateNumber)
                {
                    return true;
                }
            }
            return false;
        }

    }
}


