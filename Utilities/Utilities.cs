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

        public static List<Employee> waitingPlugInEmployees = new();
        public static List<Employee> waitingUnplugEmployees = new();

        public static List<Employee> employees = new();
        public static List<Car> CarsToChargeNow(List<Car> cars, List<ChargingStation> stations, DateTime EndTime)
        {
            int numOfStations = stations.Count;

            cars.Sort((Car x, Car y) => (int)(x.ItsBattery.CurrentLevel - y.ItsBattery.CurrentLevel));

            List<Car> bottomHalf = GetLowestBatteryLevelCars(cars, numOfStations);

            return bottomHalf;
        }

        public static List<Car> GetLowestBatteryLevelCars(List<Car> cars, int sizeOfList)
        {
            List<Car> lowerHalf = new List<Car>(cars);
            if (sizeOfList < lowerHalf.Count)
            {
                lowerHalf.RemoveRange(sizeOfList, cars.Count - sizeOfList);
            }

            return lowerHalf;
        }

        public static List<Employee> GetLowestBatteryLevelEmployees(List<Employee> employeeList, int sizeOfList) {
            List<Employee> lowerHalf = new List<Employee>(employeeList);
            
            if (sizeOfList < lowerHalf.Count)
            {
                lowerHalf.RemoveRange(sizeOfList, employeeList.Count - sizeOfList);
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
            double average = 1;
            if (cars.Count != 0)
            {
                average = 0;
                foreach (Car car in cars)
                {
                    average += car.ItsBattery.CurrentLevel / car.ItsBattery.Capacity;
                }
                average /= cars.Count;
            }

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

            foreach (Employee i in Utilities.waitingPlugInEmployees)
            {
                if (i.Name == e.Name)
                {
                    return true;
                }
            }

            foreach (Employee i in Utilities.waitingUnplugEmployees) {
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

        public static double TimeToChargeInMinutes(List<Employee> employees, double goalPercentage)
        {
            double time = 0;
            foreach(Employee employee in employees)
            {
                double capacity = employee.ItsCar.ItsBattery.Capacity;
                double currentLevel = employee.ItsCar.ItsBattery.CurrentLevel;
                time += (goalPercentage - currentLevel) / ((capacity - currentLevel) / (chargeRate / 100)); 
            }
            time /= employees.Count;
            return time;
        }

        public static bool ReachedSecondStage(List<Employee> employees)
        {
            List<Car> cars = GetCarList(employees);
            List<Car> lowerCars = GetLowestBatteryLevelCars(cars, numChargingStations);
            List<Car> upperCars = GetUpperHalfCars(cars, numChargingStations);

            double lowerAverage = GetAverageBatteryPercentage(lowerCars);
            double upperAverage = GetAverageBatteryPercentage(upperCars);

            double timeToChargeToUpperAverage = TimeToChargeInMinutes(lowerAverage, upperAverage);

            return timeToChargeToUpperAverage < 10;
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

        public static List<Employee> GetNonChargingEmployees()
        {
            List<Employee> notChargingEmployees = new List<Employee>();
            foreach (Employee employee in employees)
            {
                if (!chargingEmployees.Contains(employee))
                {
                    notChargingEmployees.Add(employee);
                }
            }

            return notChargingEmployees;
        }

        public static void UpdateNewChargeGoal()
        {
            chargeGoalPercentage = GetAverageBatteryPercentage(GetCarList(GetNonChargingEmployees()));
        }

        public static Employee GetMaxChargeEmployee(List<Employee> employees) {
            Employee maxEmployee = null;

            foreach (var e in employees) {
                if (maxEmployee == null || e.ItsCar.ItsBattery.CurrentPercentage > maxEmployee.ItsCar.ItsBattery.CurrentPercentage) {
                    maxEmployee = e;
                }
            }

            return maxEmployee;
        }

        public static Employee GetMinChargeEmployee(List<Employee> employees) {
            Employee minEmployee = null;

            foreach (var e in employees)
            {
                if (minEmployee == null || e.ItsCar.ItsBattery.CurrentPercentage < minEmployee.ItsCar.ItsBattery.CurrentPercentage)
                {
                    minEmployee = e;
                }
            }

            return minEmployee;
        }

        public static List<Employee> GetNonWaitingEmployees() {
            List<Employee> returnList = new();

            foreach (Employee e in employees) {
                if (!waitingPlugInEmployees.Contains(e) && !waitingUnplugEmployees.Contains(e)) {
                    returnList.Add(e);
                }
            }

            return returnList;
        }
    }
}