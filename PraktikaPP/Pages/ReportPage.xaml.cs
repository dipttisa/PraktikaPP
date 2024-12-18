using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PraktikaPP.Pages
{
    /// <summary>
    /// Логика взаимодействия для ReportPage.xaml
    /// </summary>
    public partial class ReportPage : Page
    {
        private int _userId;
        private List<order> _selectedOrders; // Список выделенных заказов
        private PractikaDB _context;

        // Конструктор с параметрами для передачи ID пользователя и выделенных заказов
        public ReportPage(int userId, List<order> selectedOrders)
        {
            InitializeComponent();
            _userId = userId;
            _selectedOrders = selectedOrders; // Сохраняем выделенные заказы
            _context = new PractikaDB();
            GenerateReport();
        }

        private void GenerateReport()
        {
            // Получаем заказы для текущего пользователя или используем выделенные заказы
            var orders = _selectedOrders.Any() ? _selectedOrders : _context.order.Where(o => o.user_id == _userId).ToList();

            // Группируем заказы по категориям
            var groupedOrders = orders.GroupBy(o => o.prodact.categ.category_name);

            // Создаем список элементов для отображения в ItemsControl
            var reportItems = new List<UIElement>();

            foreach (var group in groupedOrders)
            {
                // Добавляем название категории
                reportItems.Add(new TextBlock
                {
                    Text = $"Категория: {group.Key}",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 5)
                });

                // Добавляем каждый заказ в категории
                foreach (var order in group)
                {
                    reportItems.Add(new TextBlock
                    {
                        Text = $"{order.prodact.name_prod} {order.sum} р. (стоимость)",
                        Margin = new Thickness(0, 5, 0, 5)
                    });
                }
            }

            // Вычисляем итоговую сумму
            decimal totalSum = (decimal)orders.Sum(o => o.sum);
            TotalSumTextBlock.Text = $"ИТОГО: {totalSum} р.";

            // Рассчитываем скидку
            decimal discount = CalculateDiscount(totalSum);
            DiscountTextBlock.Text = $"Скидка: {discount} р.";

            // Привязываем список элементов к ItemsControl
            ReportItemsControl.ItemsSource = reportItems;
        }

        private decimal CalculateDiscount(decimal totalSum)
        {
            // Рассчитываем скидку на основе общей суммы
            decimal discountPercentage = 0;

            if (totalSum >= 300000)
            {
                discountPercentage = 0.15m;
            }
            else if (totalSum >= 50000)
            {
                discountPercentage = 0.10m;
            }
            else if (totalSum >= 10000)
            {
                discountPercentage = 0.05m;
            }

            return totalSum * discountPercentage;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Возвращаемся на предыдущую страницу
            NavigationService.GoBack();
        }
    }
}