using PraktikaPP.Pages;
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

namespace PraktikaPP
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _userId;

        // Конструктор с параметром
        public MainWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            
        }

        private void Categories_Click(object sender, RoutedEventArgs e)
        {

           MainFrame.Navigate(new CategoriesPage());

        }
        private void Products_Click(object sender, RoutedEventArgs e)
        {

            MainFrame.Navigate(new ProductsPage());

        }
        private void Order_Click(object sender, RoutedEventArgs e)
        {

            MainFrame.Navigate(new OrdersPage(_userId));

        }

        private void Report_Click(object sender, RoutedEventArgs e)
        {
             MainFrame.Navigate(new ReportPage(_userId));
        }
    }
}