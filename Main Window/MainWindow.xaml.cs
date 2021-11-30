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
using Scheduler;

namespace Main_Window
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static List<Car> chargingCars = new();
        private static int maxSize = 0;
        
        private static Mutex chargingStationsMutex = new Mutex();

        private List<Employee> employees = new();

        public MainWindow()
        {
            InitializeComponent();

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

            Employee Employee = new();
            Employee.Name = Name.Text;
            Employee.ItsCar = car;

            employees.Add(Employee);
        }

        private void CreateListBoxItem(string text, string buttonText, RoutedEventHandler clickEvent)
        {
            Button button = new();
            button.Content = buttonText;
            button.Click += clickEvent;

            Label label = new();
            label.Content = text;

            StackPanel stackPanel = new();
            stackPanel.Children.Add(label);
            stackPanel.Children.Add(button);

            ListBoxItem listBoxItem = new();
            listBoxItem.Content = stackPanel;
        }
    }
}
