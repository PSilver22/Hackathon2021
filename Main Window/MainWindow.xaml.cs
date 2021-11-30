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

        private static List<Car> chargingCars = new();
        private static int numChargingStations = 0;
        private static Mutex chargingStationsMutex = new Mutex();
        private static double chargeRate = 0;
        private static double chargeGoal;

        private static Thread? timeThread;

        private List<Employee> employees = new();

        public MainWindow()
        {
            InitializeComponent();
            main = this;

            timeThread = new Thread(() => TimeFunction());

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
            CurrentBattery.Text = "Current Batery";
        }

        private static void TimeFunction() {
            while (true)
            {
                Thread.Sleep(1000);

                if (Scheduler.GetAverageBatteryPercentage(chargingCars) >= chargeGoal)
                {
                    Scheduler.GetLowestBatterylevelCars(Scheduler.)
                }

                chargingStationsMutex.WaitOne();
                foreach (Car car in chargingCars)
                {
                    if (car != null)
                    {
                        car.ItsBattery.CurrentLevel = Math.Max(car.ItsBattery.Capacity, car.ItsBattery.CurrentLevel + (chargeRate / 60));
                    }
                }
                chargingStationsMutex.ReleaseMutex();
            }
            employees.Add(employee);
        }

        private void CreateListBoxItem(ListBox list, string text, string buttonId, string buttonText, RoutedEventHandler clickEvent)
        {
            Button button = new();
            button.Content = buttonText;
            button.Uid = buttonId;
            button.Click += clickEvent;

            Label label = new();
            label.Content = text;

            StackPanel stackPanel = new();
            stackPanel.Children.Add(label);
            stackPanel.Children.Add(button);

            ListBoxItem listBoxItem = new();
            listBoxItem.Content = stackPanel;

            list.Items.Add(listBoxItem);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            timeThread.Join();

            base.OnClosed(e);
        }

        private List<Car> CarList()
        {
            List<Car> cars = new();
            
            foreach(Employee employee in employees)
            {
                cars.Add(employee.ItsCar);
            }

            return cars;
        }

    }
}
