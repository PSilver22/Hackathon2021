﻿using System;
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

        private volatile static bool running = true;

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
            if (Name.Text == "Name")
            {
                LicensePlate.Text = "License Plate #";
                BatteryCapacity.Text = "Battery Capacity";
                CurrentBattery.Text = "Current Battery";
                return;
            }
            Battery battery = new();
            Car car = new();
            try
            {
                battery.Capacity = double.Parse(BatteryCapacity.Text);
                battery.CurrentLevel = double.Parse(CurrentBattery.Text);
                car.LicensePlateNumber = int.Parse(LicensePlate.Text);
            }
            catch(FormatException)
            {
                LicensePlate.Text = "License Plate #";
                BatteryCapacity.Text = "Battery Capacity";
                CurrentBattery.Text = "Current Battery";
                Name.Text = "Name";
                return;
            }
            
            car.ItsBattery = battery;

            Employee employee = new();
            employee.Name = Name.Text;
            employee.ItsCar = car;

            employees.Add(employee);

            waitingEmployeesMutex.WaitOne();
            
            List<Car> chargingCars = GetCarList(chargingEmployees);
            if (chargingCars.Count < numChargingStations && chargingCars.Count < employees.Count)
            {
                double? minEmployeeCharge = employees.Min(e => (chargingCars.Contains(e.ItsCar)) ? null : e.ItsCar.ItsBattery.CurrentPercentage);

                if (minEmployeeCharge is not null)
                {
                    Employee? promptEmployee = employees.Where(e => e.ItsCar.ItsBattery.CurrentPercentage == minEmployeeCharge && !WaitingForUpdate(e)).FirstOrDefault();

                    if (promptEmployee is not null)
                    {
                        CreateListBoxItem(main.UpdatedEmployees, promptEmployee.Name, "Plug in", new RoutedEventHandler(main.PlugInButton_Click));

                        awaitingUpdateEmployees.Add(promptEmployee);
                    }
                }
            }

            waitingEmployeesMutex.ReleaseMutex();

            UpdateEmployeeList();

            LicensePlate.Text = "License Plate #";
            BatteryCapacity.Text = "Battery Capacity";
            CurrentBattery.Text = "Current Battery";
            Name.Text = "Name";
        }

        private void UpdateEmployeeList() {
            main.EmployeeList.Items.Clear();

            foreach (var employee in employees) {
                ListBoxItem newItem = new ListBoxItem();
                StackPanel newContent = new StackPanel() { Name="EmployeeInfo" };
                Label nameLabel = new Label() { Name = "NameLabel", Content = employee.Name };

                newContent.Children.Add(nameLabel);

                newItem.Content = newContent;
                newItem.Selected += new RoutedEventHandler(ListBoxItem_Select);
                newItem.Unselected += new RoutedEventHandler(ListBoxItem_Deselect);
                main.EmployeeList.Items.Add(newItem);
            }
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
            awaitingUpdateEmployees = awaitingUpdateEmployees.Where(e => e.Name != ((Button)sender).Name).ToList();
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

        private static void UpdateBatterylevel(List<Employee> ChargingEmployees, double ChargeTimeInMinutes)
        {
            if(ChargeTimeInMinutes == 0)
            {
                return;
            }

            foreach(Employee employee in ChargingEmployees)
            {
                employee.ItsCar.ItsBattery.CurrentLevel = Math
                    .Min(employee.ItsCar.ItsBattery.CurrentLevel + (ChargeTimeInMinutes * chargeRate / 60),
                    employee.ItsCar.ItsBattery.Capacity);
            }
        }

        private static void UpdateChargeGoal(List<Employee> chargingEmployees, List<Employee> allEmployees) {
            foreach (Employee employee in allEmployees)
            {

            }
        }

        private static void DisplayChargingEmployees(List<Employee> EmployeesThatAreCharging)
        {
            main.Dispatcher.Invoke(() =>
            {
                main.ChargingEmployees.Items.Clear();

                foreach (Employee employee in EmployeesThatAreCharging)
                {
                    Label label = new();
                    label.Content = "Name: " + employee.Name + "\nBattery Level: " + Math.Round(employee.ItsCar.ItsBattery.CurrentPercentage, 2) + "%";
                    main.ChargingEmployees.Items.Add(label);
                }
            });
        }

        private static void TimeFunction() {
            while (running)
            { 
                Thread.Sleep(1000);
                
                List<Car> chargingCars = GetCarList(chargingEmployees);

                // if the average percentage has reached the goal
                if (Scheduler.GetAverageBatteryPercentage(chargingCars) >= chargeGoalPercentage)
                {
                    // get the next group of cars
                    List<Car> possibleChanges = Scheduler.GetLowestBatterylevelCars(GetCarList(employees), numChargingStations).Where(c => !chargingCars.Contains(c)).ToList();

                    foreach (Employee chargingEmployee in chargingEmployees) {
                        if (!WaitingForUpdate(chargingEmployee) && (!possibleChanges.Contains(chargingEmployee.ItsCar) || chargingEmployee.ItsCar.ItsBattery.CurrentPercentage == 100)) {
                            main.Dispatcher.Invoke(() => CreateListBoxItem(main.UpdatedEmployees, chargingEmployee.Name, "Unplug", new RoutedEventHandler(main.UnplugButton_Click)));

                            waitingEmployeesMutex.WaitOne();
                            awaitingUpdateEmployees.Add(chargingEmployee);
                            waitingEmployeesMutex.ReleaseMutex();
                        }
                    }
                }

                waitingEmployeesMutex.WaitOne();

                if (chargingCars.Count < numChargingStations && chargingCars.Count < employees.Count) {
                    double? minEmployeeCharge = employees.Min(e => (chargingCars.Contains(e.ItsCar)) ? null : e.ItsCar.ItsBattery.CurrentPercentage);

                    if (minEmployeeCharge is not null) {
                        Employee? promptEmployee = employees.Where(e => e.ItsCar.ItsBattery.CurrentPercentage == minEmployeeCharge && !WaitingForUpdate(e)).FirstOrDefault();

                        if (promptEmployee is not null) {
                            main.Dispatcher.Invoke(() => CreateListBoxItem(main.UpdatedEmployees, promptEmployee.Name, "Plug in", new RoutedEventHandler(main.PlugInButton_Click)));

                            awaitingUpdateEmployees.Add(promptEmployee);
                        }
                    }
                }
                waitingEmployeesMutex.ReleaseMutex();

                chargingStationsMutex.WaitOne();
                UpdateBatterylevel(chargingEmployees, 1);
                DisplayChargingEmployees(chargingEmployees);
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
            running = false;

            timeThread?.Join();
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

        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox)
            {
                ((TextBox)sender).Text = "";
            }
        }

        private void TextBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                ((TextBox)sender).Text = "";
            }
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox textBox = (TextBox)sender;

                if (textBox.Text == "")
                {
                    switch (textBox.Name)
                    {
                        case "Name": textBox.Text = "Name"; break;
                        case "LicensePlate": textBox.Text = "License Plate #"; break;
                        case "CurrentBattery": textBox.Text = "Current Battery"; break;
                        case "BatteryCapacity": textBox.Text = "Battery Capacity"; break;
                        case "NumChargingStations": textBox.Text = "# Charging Stations"; break;
                        case "ChargingRate": textBox.Text = "Charging Rate"; break;
                    }
                }
            }
        }

        private void TextBoxKeyboardLostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox textBox = (TextBox)sender;

                if (textBox.Text == "")
                {
                    switch (textBox.Name)
                    {
                        case "Name": textBox.Text = "Name"; break;
                        case "LicensePlate": textBox.Text = "License Plate #"; break;
                        case "CurrentBattery": textBox.Text = "Current Battery"; break;
                        case "BatteryCapacity": textBox.Text = "Battery Capacity"; break;
                        case "NumChargingStations": textBox.Text = "# Charging Stations"; break;
                        case "ChargingRate": textBox.Text = "Charging Rate"; break;
                    }
                }
            }
        }

        private void NumChargingStations_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Tab)
            {
                if (NumChargingStations.Text == "")
                {
                    NumChargingStations.Text = "# Charging Stations";
                    return;
                }

                TextBox textBox = (TextBox)sender;

                try
                {
                    numChargingStations = int.Parse(textBox.Text);
                }
                catch(FormatException)
                {
                    NumChargingStations.Text = "# Charging Stations";
                    return;
                }

                textBox.Visibility = Visibility.Hidden;

                Label label = new();

                label.Content = "Number of Charging Stations: " + numChargingStations;

                label.Margin = new Thickness(900.0, 300.0, 100.0, 300.0);
                label.Width = 200;
                label.Height = 30;

                MainGrid.Children.Add(label);
            }
        }

        private void ChargingRate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Tab)
            {
                if (ChargingRate.Text == "")
                {
                    ChargingRate.Text = "Charging Rate";
                    return;
                }

                TextBox textBox = (TextBox)sender;

                try
                {
                    chargeRate = int.Parse(textBox.Text);
                }
                catch(FormatException)
                {
                    ChargingRate.Text = "Charging Rate";
                    return;
                }

                textBox.Visibility = Visibility.Hidden;

                Label label = new();

                label.Content = "Charge Rate: " + chargeRate + " mAh";

                label.Margin = new Thickness(900, 350, 100, 275);
                label.Width = 200;
                label.Height = 30;

                MainGrid.Children.Add(label);
            }
        }

        private void NumChargingStations_GotFocus(object sender, RoutedEventArgs e)
        {
            NumChargingStations.Text = "";
        }

        private void NumChargingStations_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            NumChargingStations.Text = "";
        }

        private void ChargingRate_GotFocus(object sender, RoutedEventArgs e)
        {
            ChargingRate.Text = "";
        }

        private void ChargingRate_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChargingRate.Text = "";
        }

        private void NumChargingStations_LostFocus(object sender, RoutedEventArgs e)
        {
            if (NumChargingStations.Text == "")
            {
                NumChargingStations.Text = "# Charging Stations";
            }
        }

        private void NumChargingStations_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (NumChargingStations.Text == "")
            {
                NumChargingStations.Text = "# Charging Stations";
            }
        }

        private void ChargingRate_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ChargingRate.Text == "")
            {
                ChargingRate.Text = "Charging Rate";
            }
        }

        private void ChargingRate_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (ChargingRate.Text == "")
            {
                ChargingRate.Text = "Charging Rate";
            }
        }

        private void ListBoxItem_Select(object sender, RoutedEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)sender;

            if (item is not null) {
                StackPanel content = (StackPanel)item.Content;
                string employeeName = content.Children[0].ToString().Split(" ")[1];

                Employee? updateEmployee = employees.Find(e => e.Name == employeeName);

                content.Children.Add(new Label() { Content = (Math.Round(updateEmployee.ItsCar.ItsBattery.CurrentPercentage, 2)).ToString() + "%", Visibility = Visibility.Visible });
            }
        }

        private void ListBoxItem_Deselect(object sender, RoutedEventArgs e) {
            ListBoxItem item = (ListBoxItem)sender;

            if (item is not null)
            {
                StackPanel content = (StackPanel)item.Content;
                content.Children.RemoveAt(1);
                item.Content = content;
            }
        }
    }
}
