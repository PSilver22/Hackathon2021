using System;
using System.Collections.Generic;
using System.Threading;
using Data_Structures;

namespace Utilities_ns
{
    public class Utilities
    {
        public static int numChargingStations = 2;
        public static double chargeRate = 60;
        public static double chargeGoalPercentage = 10;

        public static List<Employee> employees = new();
        
        
        public static double GetAverageBatteryPercentage(BatteryState batteryState)
        {
            double average = 1;

            List<Car> cars = GetCarListOfState(batteryState);

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

        public static List<Employee> GetLowestBatteryLevelEmployees(BatteryState state, int num) {
            List<Employee> minEmployees = new();

            foreach (var employee in EmployeesInState(state)) {
                minEmployees.Add(employee);
            }

            minEmployees.Sort((x, y) => {
                double result = x.ItsCar.ItsBattery.CurrentPercentage - y.ItsCar.ItsBattery.CurrentPercentage;
                if (result < 0) { return -1; }
                else if (result > 0) { return 1; }
                else { return 0; }
                });

            if (num < minEmployees.Count)
            {
                minEmployees.RemoveRange(num, minEmployees.Count - num);
            }

            return minEmployees;
        }

        public static void UpdateBatterylevel(double ChargeTimeInMinutes)
        {
            if (ChargeTimeInMinutes == 0)
            {
                return;
            }

            foreach (Employee employee in EmployeesInState(BatteryState.charging))
            {
                employee.ItsCar.ItsBattery.CurrentLevel = Math
                    .Min(employee.ItsCar.ItsBattery.CurrentLevel + (ChargeTimeInMinutes * chargeRate / 60),
                    employee.ItsCar.ItsBattery.Capacity);
            }
        }

        public static bool WaitingForUpdate(Employee e)
        {
            if (e.ItsCar.ItsBattery.State == BatteryState.waitingToCharge || e.ItsCar.ItsBattery.State == BatteryState.waitingToNotCharge)
            {
                return true;
            }
            return false;
        }

        public static List<Car> GetCarListOfState(BatteryState batteryState)
        {
            List<Car> cars = new();

            foreach (Employee employee in EmployeesInState(batteryState))
            {
                cars.Add(employee.ItsCar);
            }

            return cars;
        }

        public static double TimeToChargeInMinutes(List<Employee> employees, double goalPercentage)
        {
            double time = 0;
            foreach (Employee employee in employees)
            {
                double percentageTime = (employee.ItsCar.ItsBattery.Capacity / 100) / (Utilities.chargeRate/60);
                time += percentageTime * (goalPercentage - employee.ItsCar.ItsBattery.CurrentPercentage);
            }
            time /= employees.Count;
            return time;
        }

        public static bool ReachedSecondStage()
        {
            
            double timeToChargeToChargeGoal = TimeToChargeInMinutes(Utilities.EmployeesInState(BatteryState.charging), chargeGoalPercentage);

            return timeToChargeToChargeGoal < 120;
        }

        public static bool CarExists(int LicensePlateNumber)
        {
            foreach (Employee employee in employees)
            {
                if (employee.ItsCar.LicensePlateNumber == LicensePlateNumber)
                {
                    return true;
                }
            }
            return false;
        }

        public static void UpdateNewChargeGoal()
        {
            chargeGoalPercentage = GetAverageBatteryPercentage(BatteryState.notCharging);
        }

        public static Employee GetMinStateEmployee(BatteryState batteryState)
        {
            Employee minEmployee = null;

            List<Employee> EmployeesInThatState = EmployeesInState(batteryState);

            EmployeesInThatState.Sort((Employee x, Employee y) => (int)(x.ItsCar.ItsBattery.CurrentPercentage - y.ItsCar.ItsBattery.CurrentPercentage));

            return EmployeesInThatState[0];
        }

        public static Employee GetMaxStateEmployee(BatteryState batteryState)
        {
            Employee maxEmployee = null;

            List<Employee> EmployeesInThatState = EmployeesInState(batteryState);

            if (EmployeesInThatState.Count == 0) {
                return null;
            }

            EmployeesInThatState.Sort((Employee x, Employee y) => (int)(x.ItsCar.ItsBattery.CurrentPercentage - y.ItsCar.ItsBattery.CurrentPercentage));

            return EmployeesInThatState[^1];
        }

        public static List<Employee> EmployeesInState(BatteryState batteryState)
        {
            if (batteryState == BatteryState.allStates) {
                return employees;
            }

            List<Employee> employeesInState = new();

            foreach (Employee employee in employees)
            {
                if (employee.ItsCar.ItsBattery.State == batteryState)
                {
                    employeesInState.Add(employee);
                }
            }
            return employeesInState;
        }

        public static int NumOfEmployeesinState(BatteryState batteryState)
        {
            return EmployeesInState(batteryState).Count;
        }
    }
}