using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Data_Structures;
using Scheduler_ns;

namespace Main_Window
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow main;

        private static List<Employee> chargingEmployees = new();
        private static int numChargingStations = 3;
        private static Mutex chargingStationsMutex = new Mutex();
        private static double chargeRate = 60;
        private static double chargeGoalPercentage = 10;

        private static Mutex waitingEmployeesMutex = new Mutex();
        private static List<Employee> awaitingUpdateEmployees = new();

        private static Thread? timeThread;

        private static List<Employee> employees = new();

        public MainWindow()
        {
            InitializeComponent();
            main = this;

            timeThread = new Thread(() => TimeFunction());

            timeThread.Start();
            //ListBoxItem newItem = new ListBoxItem();
            //StackPanel newItemContent = new StackPanel();

            //Label newLabel = new Label();
            //newLabel.Content = "Hello";

            //Button button = new Button();
            //button.Content = "Test";

            //newItemContent.Children.Add(newLabel);
            //newItemContent.Children.Add(button);

            //newItem.Content = newItemContent;

            //UpdatedEmployees.Items.Add(newItem)"
        }

        private void AddEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            Battery battery = new();

            battery.Capacity = double.Parse(BatteryCapacity.Text);
            battery.CurrentLevel = double.Parse(CurrentBattery.Text);

            Car car = new();
            car.LicensePlateNumber = int.Parse(LicensePlate.Text);
            car.ItsBattery = battery;

            Employee employee = new();
            employee.Name = Name.Text;
            employee.ItsCar = car;

            employees.Add(employee);

            LicensePlate.Text = "License Plate #";
            BatteryCapacity.Text = "Battery Capacity";
            CurrentBattery.Text = "Current Battery";
        }

        private int GetItemIndex(ListBox list, string name)
        {
            for (int i = 0; i < list.Items.Count; i++)
            {
                if (((ListBoxItem)list.Items.GetItemAt(i)).Name == name)
                    return i;
            }

            return -1;
        }

        private void UnplugButton_Click(object sender, RoutedEventArgs e)
        {
            chargingEmployees = chargingEmployees.Where(e => e.Name != ((Button)sender).Name).ToList();

            waitingEmployeesMutex.WaitOne();
            awaitingUpdateEmployees.Where(e => e.Name != ((Button)sender).Name);
            UpdatedEmployees.Items.RemoveAt(GetItemIndex(UpdatedEmployees, ((Button)sender).Name));
            waitingEmployeesMutex.ReleaseMutex();
        }

        private void PlugInButton_Click(object sender, RoutedEventArgs e) {
            Employee? newChargeEmployee = employees.Find(e => e.Name == ((Button) sender).Name);

            if (newChargeEmployee != null) {
                chargingEmployees.Add(newChargeEmployee);

                waitingEmployeesMutex.WaitOne();
                awaitingUpdateEmployees.Remove(newChargeEmployee);
                UpdatedEmployees.Items.RemoveAt(GetItemIndex(UpdatedEmployees, ((Button) sender).Name));
                waitingEmployeesMutex.ReleaseMutex();
            }
        }

        private static void TimeFunction() {

            while (true)
            {
                Thread.Sleep(1000);

                List<Car> chargingCars = GetCarList(chargingEmployees);
                if (Scheduler.GetAverageBatteryPercentage(chargingCars) >= chargeGoalPercentage)
                {
                    List<Car> possibleChanges = Scheduler.GetUpperHalfCars(GetCarList(employees), numChargingStations);

                    foreach (Employee chargingEmployee in chargingEmployees) {
                        if (!possibleChanges.Contains(chargingEmployee.ItsCar) && !WaitingForUpdate(chargingEmployee)) {
                            main.Dispatcher.Invoke(() => CreateListBoxItem(main.UpdatedEmployees, chargingEmployee.Name, "Unplug", new RoutedEventHandler(main.UnplugButton_Click)));

                            waitingEmployeesMutex.WaitOne();
                            awaitingUpdateEmployees.Add(chargingEmployee);
                            waitingEmployeesMutex.ReleaseMutex();
                        }
                    }
                }

                if (chargingCars.Count < numChargingStations && chargingCars.Count < employees.Count) {
                    double? minEmployeeCharge = employees.Min(e => (chargingCars.Contains(e.ItsCar)) ? null : e.ItsCar.ItsBattery.CurrentPercentage);

                    if (minEmployeeCharge is not null) {
                        Employee? promptEmployee = employees.Where(e => e.ItsCar.ItsBattery.CurrentPercentage == minEmployeeCharge && !WaitingForUpdate(e)).FirstOrDefault();

                        if (promptEmployee is not null) {
                            main.Dispatcher.Invoke(() => CreateListBoxItem(main.UpdatedEmployees, promptEmployee.Name, "Plug in", new RoutedEventHandler(main.PlugInButton_Click)));

                            waitingEmployeesMutex.WaitOne();
                            awaitingUpdateEmployees.Add(promptEmployee);
                            waitingEmployeesMutex.ReleaseMutex();
                        }
                    }
                }

                chargingStationsMutex.WaitOne();
                foreach (Car car in chargingCars)
                {
                    if (car != null)
                    {
                        car.ItsBattery.CurrentLevel = Math.Min(car.ItsBattery.Capacity, car.ItsBattery.CurrentLevel + (chargeRate / 60));
                    }
                }
                chargingStationsMutex.ReleaseMutex();
            }
        }

        private static void CreateListBoxItem(ListBox list, string text, string buttonText, RoutedEventHandler clickEvent)
        {
            Button button = new();
            button.Content = buttonText;
            button.Name = text;
            button.Click += clickEvent;

            Label label = new();
            label.Content = text;

            StackPanel stackPanel = new();
            stackPanel.Children.Add(label);
            stackPanel.Children.Add(button);

            ListBoxItem listBoxItem = new();
            listBoxItem.Content = stackPanel;
            listBoxItem.Name = text;

            list.Items.Add(listBoxItem);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            timeThread?.Join();

            base.OnClosed(e);
        }

        private static bool WaitingForUpdate(Employee e) {
            if (e == null) {
                return false;
            }

            foreach (Employee i in awaitingUpdateEmployees) {
                if (i.Name == e.Name) {
                    return true;
                }
            }

            return false;
        }

        private static List<Car> GetCarList(List<Employee> employeeList)
        {
            List<Car> cars = new();
            
            foreach(Employee employee in employeeList)
            {
                cars.Add(employee.ItsCar);
            }

            return cars;
        }

    }
}
