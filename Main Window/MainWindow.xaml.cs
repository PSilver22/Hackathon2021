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


    }
}
