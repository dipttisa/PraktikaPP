using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ClosedXML.Excel;

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

        // Метод для экспорта данных в Excel на рабочий стол
        private void DonlowdButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем заказы для текущего пользователя или используем выделенные заказы
                var orders = _selectedOrders.Any() ? _selectedOrders : _context.order.Where(o => o.user_id == _userId).ToList();

                // Группируем заказы по категориям
                var groupedOrders = orders.GroupBy(o => o.prodact.categ.category_name);

                // Создаем Excel-файл
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Отчет");

                    // Заголовки столбцов
                    worksheet.Cell(1, 1).Value = "Категория";
                    worksheet.Cell(1, 2).Value = "Продукт";
                    worksheet.Cell(1, 3).Value = "Сумма";
                    worksheet.Cell(1, 4).Value = "Дата";

                    int rowIndex = 2; // Начинаем с 2 строки для данных

                    foreach (var group in groupedOrders)
                    {
                        // Добавляем название категории
                        worksheet.Cell(rowIndex, 1).Value = group.Key;
                        worksheet.Cell(rowIndex, 1).Style.Font.Bold = true;
                        rowIndex++;

                        // Добавляем каждый заказ в категории
                        foreach (var order in group)
                        {
                            worksheet.Cell(rowIndex, 2).Value = order.prodact.name_prod;
                            worksheet.Cell(rowIndex, 3).Value = order.sum;
                            worksheet.Cell(rowIndex, 4).Value = order.date.ToString("dd.MM.yyyy");
                            rowIndex++;
                        }
                    }

                    // Вычисляем итоговую сумму
                    decimal totalSum = (decimal)orders.Sum(o => o.sum);
                    worksheet.Cell(rowIndex, 1).Value = "ИТОГО";
                    worksheet.Cell(rowIndex, 1).Style.Font.Bold = true;
                    worksheet.Cell(rowIndex, 3).Value = totalSum;

                    // Рассчитываем скидку
                    decimal discount = CalculateDiscount(totalSum);
                    worksheet.Cell(rowIndex + 1, 1).Value = "Скидка";
                    worksheet.Cell(rowIndex + 1, 1).Style.Font.Bold = true;
                    worksheet.Cell(rowIndex + 1, 3).Value = discount;

                    // Сохраняем файл на рабочий стол
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string fileName = "Отчет.xlsx";
                    string filePath = Path.Combine(desktopPath, fileName);

                    // Проверяем, существует ли файл, и удаляем его, если он существует
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    workbook.SaveAs(filePath);

                    MessageBox.Show($"Отчет успешно сохранен на рабочий стол: {filePath}", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}