using PraktikaPP.DopPages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<order> Orders { get; set; }
        private PractikaDB _context;
        private int _userId; // ID текущего пользователя

        // Конструктор с параметром для передачи ID пользователя
        public OrdersPage(int userId)
        {
            InitializeComponent();
            _context = PractikaDB.GetContext();
            _userId = userId; // Сохраняем ID пользователя
            Orders = new ObservableCollection<order>(_context.order.Where(o => o.user_id == _userId).ToList()); // Загружаем только заказы текущего пользователя
            LoadData();
            LoadCategories();
        }

        private void LoadData()
        {
            OrdersGrid.ItemsSource = Orders;
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
            

            // Создаем новый объект заказа с автоматически сгенерированным ID
            var newOrder = new order
            {
                id = 0,
                product_id = 0, // Установите ID продукта по умолчанию
                count = 0, // Установите количество по умолчанию
                user_id = _userId, // Устанавливаем ID текущего пользователя
                date = DateTime.Now,
                sum = 0, // Установите сумму по умолчанию
                price = 0
            };
            _context.order.Add(newOrder);

           

            // Переходим на страницу редактирования продукта
            NavigationService.Navigate(new OrderEditPage(newOrder, _userId));
        }

        public void OnOrderUpdated()
        {
            // Обновляем список заказов только для текущего пользователя
            LoadOrders();
        }

        private void LoadOrders()
        {
            var orders = _context.order.Where(o => o.user_id == _userId).ToList();
            OrdersGrid.ItemsSource = orders; // Перезагружаем источник данных для Grid
        }

        private void ClearOrders_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void PoickOrders_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            var selectedOrders = OrdersGrid.SelectedItems.Cast<order>().ToList();

            if (selectedOrders.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы один заказ для формирования отчета.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Переходим на страницу ReportPage с выделенными заказами
            NavigationService.Navigate(new ReportPage(_userId, selectedOrders));
        }

        private void ApplyFilter()
        {
            var selectedCategory = CategoryComboBox.SelectedItem as categ;
            var startDate = StartDatePicker.SelectedDate;
            var endDate = EndDatePicker.SelectedDate;

            var query = _context.order.Where(o => o.user_id == _userId).AsQueryable(); // Фильтруем по ID пользователя

            // Фильтрация по категории
            if (selectedCategory != null && selectedCategory.id != 0)
            {
                query = query.Where(o => o.prodact.id_cat == selectedCategory.id);
            }

            // Загружаем данные из базы данных
            var filteredOrders = query.ToList();

            // Фильтрация по дате начала (в памяти)
            if (startDate.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.date >= startDate.Value).ToList();
            }

            // Фильтрация по дате окончания (в памяти)
            if (endDate.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.date <= endDate.Value).ToList();
            }

            // Применение фильтрации и обновление DataGrid
            OrdersGrid.ItemsSource = filteredOrders;
        }
    }
}