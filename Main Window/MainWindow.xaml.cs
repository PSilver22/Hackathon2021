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
using Utilities_ns;
using EmailSender_ns;

namespace Main_Window
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow main;

        private static EmailSender EmailSender = new();
        
        private static Thread? timeThread;

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
            if (Name.Text == "Name" || EmailAddress.Text == "Email Address")
            {
                ResetLabels();
                return;
            }

            Battery battery = new();
            Car car = new();
            try
            {
                battery.Capacity = double.Parse(BatteryCapacity.Text);
                battery.CurrentLevel = double.Parse(CurrentBattery.Text);
                int licensePlateNumber = int.Parse(LicensePlate.Text);
                if (Utilities.CarExists(licensePlateNumber))
                {
                    ResetLabels();
                    return;
                }
                car.LicensePlateNumber = licensePlateNumber;
            }
            catch(FormatException)
            {
                ResetLabels();
                return;
            }
            
            car.ItsBattery = battery;

            Employee employee = new();
            employee.Name = Name.Text.Replace('_', ' ');
            employee.ItsCar = car;

            string emailAddress = EmailAddress.Text;
            if (!EmailSender.IsValidEmail(emailAddress))
            {
                ResetLabels();
                return;
            }

            employee.EmailAdress = emailAddress;

            Utilities.employees.Add(employee);

            Utilities.waitingEmployeesMutex.WaitOne();
            
            List<Car> chargingCars = Utilities.GetCarList(Utilities.chargingEmployees);

            // get the employees with the smallest charge
            List<Employee> minEmployees = Utilities.GetLowestBatteryLevelEmployees(Utilities.GetNonChargingEmployees(), Math.Min(Utilities.employees.Count - chargingCars.Count, Utilities.numChargingStations - chargingCars.Count));

            if (chargingCars.Count < Utilities.numChargingStations && chargingCars.Count < Utilities.employees.Count)
            {
                if (Utilities.waitingPlugInEmployees.Count < Utilities.numChargingStations && Utilities.waitingPlugInEmployees.Count < Utilities.employees.Count)
                {
                    foreach (Employee minEmployee in minEmployees)
                    {
                        if (!Utilities.waitingPlugInEmployees.Contains(minEmployee) && !Utilities.waitingUnplugEmployees.Contains(minEmployee))
                        {
                            Utilities.waitingPlugInEmployees.Add(minEmployee);
                        }
                    }
                }

                else
                {
                    if (!Utilities.GetLowestBatteryLevelEmployees(Utilities.GetNonWaitingEmployees(), Math.Min(Utilities.employees.Count - chargingCars.Count, Utilities.numChargingStations - chargingCars.Count)).Contains(Utilities.GetMaxChargeEmployee(Utilities.waitingPlugInEmployees)))
                    {
                        Utilities.waitingPlugInEmployees.Remove(Utilities.GetMaxChargeEmployee(Utilities.waitingPlugInEmployees));
                        Utilities.waitingPlugInEmployees.Add(Utilities.GetMinChargeEmployee(Utilities.GetNonWaitingEmployees()));
                    }
                }
                UpdateUpdatedEmployees();
            }

            Utilities.waitingEmployeesMutex.ReleaseMutex();

            Utilities.UpdateNewChargeGoal();

            UpdateEmployeeList();

            ResetLabels();
        }

        private void ResetLabels()
        {
            Name.Text = "Name";
            EmailAddress.Text = "Email Address";
            LicensePlate.Text = "License Plate #";
            BatteryCapacity.Text = "Battery Capacity";
            CurrentBattery.Text = "Current Battery";
        }

        private void UpdateEmployeeList() {
            main.EmployeeList.Items.Clear();

            foreach (var employee in Utilities.employees) {
                ListBoxItem newItem = new ListBoxItem();
                StackPanel newContent = new StackPanel() { Name="EmployeeInfo" };
                Label nameLabel = new Label() { Name = "NameLabel", Content = employee.Name.Replace('_', ' ') };

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
                if (((ListBoxItem)list.Items.GetItemAt(i)).Name == name.Replace(' ', '_'))
                    return i;
            }

            return -1;
        }

        private void UnplugButton_Click(object sender, RoutedEventArgs e)
        {
            Utilities.chargingEmployees = Utilities.chargingEmployees.Where(e => e.Name != ((Button)sender).Name.Replace('_', ' ')).ToList();

            Utilities.UpdateNewChargeGoal();

            Utilities.waitingEmployeesMutex.WaitOne();
            Utilities.waitingUnplugEmployees.Where(e => e.Name != ((Button)sender).Name.Replace('_', ' '));
            UpdatedEmployees.Items.RemoveAt(GetItemIndex(UpdatedEmployees, ((Button)sender).Name));
            Utilities.waitingEmployeesMutex.ReleaseMutex();

            ETA.Content = "Charging estimated end time: " + DateTime.Now.AddMinutes(Utilities.TimeToChargeInMinutes(Utilities.GetAverageBatteryPercentage(Utilities.GetCarList(Utilities.chargingEmployees)), Utilities.chargeGoalPercentage)).ToString();
        }

        private void PlugInButton_Click(object sender, RoutedEventArgs e) {
            Employee? newChargeEmployee = Utilities.employees.Find(e => e.Name == ((Button) sender).Name.Replace('_', ' '));

            if (newChargeEmployee != null) {
                Utilities.chargingEmployees.Add(newChargeEmployee);

                Utilities.UpdateNewChargeGoal();

                SendExpectedUnplugTime(newChargeEmployee, )

                Utilities.waitingEmployeesMutex.WaitOne();
                Utilities.waitingPlugInEmployees.Remove(newChargeEmployee);
                UpdatedEmployees.Items.RemoveAt(GetItemIndex(UpdatedEmployees, ((Button) sender).Name));
                Utilities.waitingEmployeesMutex.ReleaseMutex();

                ETA.Content = "Charging estimated end time: " + DateTime.Now.AddMinutes(Utilities.TimeToChargeInMinutes(Utilities.GetAverageBatteryPercentage(Utilities.GetCarList(Utilities.chargingEmployees)), Utilities.chargeGoalPercentage)).ToString();
            }
        }

        private static void DisplayChargingEmployees(List<Employee> EmployeesThatAreCharging)
        {
            try
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
            catch (Exception) {}
        }

        private static void TimeFunction() {
            int timePassed = 0;

            while (running)
            { 
                Thread.Sleep(1000);
                
                List<Car> chargingCars = Utilities.GetCarList(Utilities.chargingEmployees);

                if (!Utilities.ReachedSecondStage(Utilities.employees))
                {
                    timePassed = 0;

                    // if the average percentage has reached the goal
                    if (Utilities.GetAverageBatteryPercentage(chargingCars) >= Utilities.chargeGoalPercentage)
                    {
                        // get the next group of cars
                        List<Employee> possibleChanges = Utilities.GetLowestBatteryLevelEmployees(Utilities.employees, Utilities.numChargingStations);

                        // check each charging employee
                        foreach (Employee chargingEmployee in Utilities.chargingEmployees)
                        {
                            // if the employee is not already waiting to be updated and (the car doesn't still need charge or the cars battery is at 100)
                            if (!Utilities.WaitingForUpdate(chargingEmployee) && (!possibleChanges.Contains(chargingEmployee) || chargingEmployee.ItsCar.ItsBattery.CurrentPercentage == 100))
                            {
                                // add the employee to list of employees waiting for an update
                                Utilities.waitingEmployeesMutex.WaitOne();
                                Utilities.waitingUnplugEmployees.Add(chargingEmployee);
                                UpdateUpdatedEmployees();
                                Utilities.waitingEmployeesMutex.ReleaseMutex();
                            }
                        }
                    }

                    Utilities.waitingEmployeesMutex.WaitOne();

                    // if the number of cars charging is less than the number of chargers and the number of cars charging is less than the number of total cars
                    if (chargingCars.Count < Utilities.numChargingStations && chargingCars.Count < Utilities.employees.Count)
                    {
                        // get the employees with the smallest charge
                        List<Employee> minEmployees = Utilities.GetLowestBatteryLevelEmployees(Utilities.GetNonChargingEmployees(), Math.Min(Utilities.employees.Count - chargingCars.Count, Utilities.numChargingStations - chargingCars.Count));

                        foreach (Employee employee in minEmployees)
                        {
                            if (!Utilities.waitingPlugInEmployees.Contains(employee))
                            {
                                Utilities.waitingPlugInEmployees.Add(employee);
                                UpdateUpdatedEmployees();
                            }
                        }
                    }
                    Utilities.waitingEmployeesMutex.ReleaseMutex();
                }

                else {
                    ++timePassed;

                    if (timePassed >= 120) {
                        List<Employee> possibleChanges = Utilities.GetLowestBatteryLevelEmployees(Utilities.employees, Utilities.numChargingStations);

                        foreach (Employee employee in Utilities.chargingEmployees) {
                            if (!Utilities.WaitingForUpdate(employee) && (!possibleChanges.Contains(employee) || employee.ItsCar.ItsBattery.CurrentPercentage == 100))
                            {
                                Utilities.waitingEmployeesMutex.WaitOne();
                                Utilities.waitingUnplugEmployees.Add(employee);
                                UpdateUpdatedEmployees();
                                Utilities.waitingEmployeesMutex.ReleaseMutex();
                            }
                        }

                        if (Utilities.chargingEmployees.Count < Utilities.numChargingStations) {
                            List<Employee> minEmployees = Utilities.GetLowestBatteryLevelEmployees(Utilities.employees, Utilities.numChargingStations - Utilities.chargingEmployees.Count);

                            foreach (Employee e in minEmployees) {
                                if (!Utilities.WaitingForUpdate(e)) {
                                    Utilities.waitingEmployeesMutex.WaitOne();
                                    Utilities.waitingPlugInEmployees.Add(e);
                                    UpdateUpdatedEmployees();
                                    Utilities.waitingEmployeesMutex.ReleaseMutex();
                                }
                            }
                        }
                    }
                }

                Utilities.chargingStationsMutex.WaitOne();

                Utilities.UpdateBatterylevel(Utilities.chargingEmployees, 1);
                DisplayChargingEmployees(Utilities.chargingEmployees);

                Utilities.chargingStationsMutex.ReleaseMutex();
            }
        }

        private static void UpdateUpdatedEmployees() {
            main.Dispatcher.Invoke(() => {
                main.UpdatedEmployees.Items.Clear();

                foreach (Employee waitingEmployee in Utilities.waitingPlugInEmployees) {
                    CreateListBoxItem(main.UpdatedEmployees, waitingEmployee.Name.Replace('_', ' '), "Plug in", new RoutedEventHandler(main.PlugInButton_Click));
                }

                foreach (Employee waitingEmployee in Utilities.waitingUnplugEmployees)
                {
                    CreateListBoxItem(main.UpdatedEmployees, waitingEmployee.Name.Replace('_', ' '), "Unplug", new RoutedEventHandler(main.UnplugButton_Click));
                }
            });
        }

        private static void CreateListBoxItem(ListBox list, string text, string buttonText, RoutedEventHandler clickEvent)
        {
            Button button = new();
            button.Content = buttonText;
            button.Name = text.Replace(' ', '_');
            button.Click += clickEvent;

            Label label = new();
            label.Content = text;

            StackPanel stackPanel = new();
            stackPanel.Children.Add(label);
            stackPanel.Children.Add(button);

            ListBoxItem listBoxItem = new();
            listBoxItem.Content = stackPanel;
            listBoxItem.Name = text.Replace(' ', '_');

            list.Items.Add(listBoxItem);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            running = false;

            try
            {
                timeThread.Join(100);
            }
            catch (Exception) { }
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
                        case "EmailAddress": textBox.Text = "Email Address"; break;
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
                        case "EmailAddress": textBox.Text = "Email Address"; break;
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
                    Utilities.numChargingStations = int.Parse(textBox.Text);
                }
                catch(FormatException)
                {
                    NumChargingStations.Text = "# Charging Stations";
                    return;
                }

                textBox.Visibility = Visibility.Hidden;

                Label label = new();

                label.Content = "Number of Charging Stations: " + Utilities.numChargingStations;

                label.Margin = new Thickness(901.5, 325, 00, 000);
                label.Width = 175;
                label.Height = 30;
                label.HorizontalAlignment = HorizontalAlignment.Left;
                label.VerticalAlignment = VerticalAlignment.Top;
                
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
                    Utilities.chargeRate = int.Parse(textBox.Text);
                }
                catch(FormatException)
                {
                    ChargingRate.Text = "Charging Rate";
                    return;
                }

                textBox.Visibility = Visibility.Hidden;

                Label label = new();

                label.Content = "Charge Rate: " + Utilities.chargeRate + " mAh";

                label.Margin = new Thickness(901.5, 350, 0, 0);
                label.Width = 120;
                label.Height = 30;
                label.HorizontalAlignment = HorizontalAlignment.Left;
                label.VerticalAlignment = VerticalAlignment.Top;

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

                Employee? updateEmployee = Utilities.employees.Find(e => e.Name == employeeName);

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

        private static string UnplugCarEmailBody(int licensePlateNumber)
        {
            return "License Plate #" + licensePlateNumber + " Please unplug your car";
        }

        private static string PluginCarEmailBody(int licensePlateNumber)
        {
            return "License Plate #" + licensePlateNumber + " Please plug in your car";
        }

        private static string CarEmailSubject()
        {
            return "Update to your cars status in the charging station";
        }

        private static void SendExpectedUnplugTime(List<Employee> employees, double GoalAverage)
        {
            double expectedtime = Utilities.TimeToChargeInMinutes(loweraverage, upperaverage);

            EmailSender.SendEmail(employee.EmailAdress, ETAEmailSubject(), ETAEmailBody(expectedtime));
        }

        private static string ETAEmailBody(double ETA)
        {
            return "Thank you for plugging in your car.\n" +
                "You can expect to unplug your car in " + ETA
                + " minutes.";
        }

        private static string ETAEmailSubject()
        {
            return "ETA for unplug";
        }
    }
}
