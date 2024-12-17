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

namespace PraktikaPP.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrdersPage.xaml
    /// </summary>
    public partial class OrdersPage : Page
    {
        private PractikaDB _context;
        public OrdersPage()
        {
            InitializeComponent();
            _context = PractikaDB.GetContext();
            LoadData();
            LoadCategories();
        }

        private void LoadData()
        {
            OrdersGrid.ItemsSource = _context.order.ToList();
        }

        private void LoadCategories()
        {
            var categories = _context.categ.ToList();
            categories.Insert(0, new categ { id = 0, category_name = "Все категории" });
            CategoryComboBox.ItemsSource = categories;
            CategoryComboBox.DisplayMemberPath = "category_name";
            CategoryComboBox.SelectedValuePath = "id";
            CategoryComboBox.SelectedIndex = 0;
        }

        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            // Логика для добавления нового заказа
        }

        private void RemoveOrder_Click(object sender, RoutedEventArgs e)
        {
            // Логика для удаления заказа
        }

        private void FilterOrders_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void ClearOrders_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            // Логика для генерации отчета
        }

        private void ApplyFilter()
        {
            var selectedCategory = CategoryComboBox.SelectedItem as categ;
            var startDate = StartDatePicker.SelectedDate;
            var endDate = EndDatePicker.SelectedDate;

            var query = _context.order.AsQueryable();

            // Фильтрация по категории
            if (selectedCategory != null && selectedCategory.id != 0)
            {
                query = query.Where(o => o.prodact.id_cat == selectedCategory.id);
            }

            // Фильтрация по дате начала
            if (startDate.HasValue)
            {
                query = query.Where(o => o.date >= startDate.Value);
            }

            // Фильтрация по дате окончания
            if (endDate.HasValue)
            {
                query = query.Where(o => o.date <= endDate.Value);
            }

            
            // Применение фильтрации и обновление DataGrid
            OrdersGrid.ItemsSource = query.ToList();
        }
    }
}