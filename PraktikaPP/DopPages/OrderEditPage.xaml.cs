using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PraktikaPP.DopPages
{
    public partial class OrderEditPage : Page
    {
        private order _currentOrder; // Текущий заказ для редактирования или добавления
        private Action _onOrderUpdated; // Колбэк для обновления данных в OrdersPage
        private PractikaDB _context;

        public OrderEditPage(order order, Action onOrderUpdated)
        {
            InitializeComponent();
            _context = PractikaDB.GetContext();
            _currentOrder = order;
            _onOrderUpdated = onOrderUpdated;

            // Загружаем данные в ComboBox
            LoadCategories();

            // Устанавливаем начальные значения для полей
            if (_currentOrder.product_id > 0)
            {
                ProductComboBox.SelectedValue = _currentOrder.product_id;
            }

            // Устанавливаем начальное значение для цены
            PriceTextBox.Text = _currentOrder.price.ToString();

            // Подписываемся на событие изменения выбранной категории
            CategoryComboBox.SelectionChanged += CategoryComboBox_SelectionChanged;
        }

        private void LoadProducts()
        {
            var products = _context.prodact.ToList();
            ProductComboBox.ItemsSource = products;
            ProductComboBox.DisplayMemberPath = "name_prod";
            ProductComboBox.SelectedValuePath = "id";
        }

        private void LoadCategories()
        {
            var categories = _context.categ.ToList();
            CategoryComboBox.ItemsSource = categories;
            CategoryComboBox.DisplayMemberPath = "category_name";
            CategoryComboBox.SelectedValuePath = "id"; // Исправлено на "id"
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Получаем выбранную категорию
            var selectedCategory = CategoryComboBox.SelectedItem as categ;

            if (selectedCategory != null)
            {
                // Фильтруем продукты по выбранной категории
                var filteredProducts = _context.prodact
                    .Where(p => p.id_cat == selectedCategory.id)
                    .ToList();

                // Обновляем источник данных для ProductComboBox
                ProductComboBox.ItemsSource = filteredProducts;
            }
            else
            {
                // Если категория не выбрана, показываем все продукты
                LoadProducts();
            }
        }

        private void KolvotNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateTotal();
        }

        private void PriceTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateTotal();
        }

        private void CalculateTotal()
        {
            // Проверяем, что количество введено
            if (!int.TryParse(KolvotNameTextBox.Text, out int quantity))
            {
                SumNameTextBox.Text = "0";
                return;
            }

            // Проверяем, что цена указана
            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Цена заказа не указана или не является корректным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                SumNameTextBox.Text = "0";
                return;
            }

            // Рассчитываем сумму
            decimal total = price * quantity;

            // Обновляем текстовое поле с суммой
            SumNameTextBox.Text = total.ToString();
        }

        private void SaveProduct_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, что все поля заполнены
            if (ProductComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите продукт!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(KolvotNameTextBox.Text, out int quantity))
            {
                
            }

            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Цена заказа не указана или не является корректным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Обновляем текущий заказ с новыми значениями
            _currentOrder.product_id = (int)ProductComboBox.SelectedValue;
            _currentOrder.count = quantity;
            _currentOrder.price = price;  // Цена берется из текстового поля
            _currentOrder.sum = price * quantity; // Сумма вычисляется на основе цены и количества
            _currentOrder.date = DateTime.Now; // Обновляем дату

            try
            {
                // Если заказ новый, добавляем его в базу данных
                if (_currentOrder.id == 0)
                {
                    _currentOrder.id = GenerateNewOrderId(); // Генерируем новый ID
                    _context.order.Add(_currentOrder); // Добавляем новый заказ в базу
                }
                else
                {
                    // Если заказ существует, обновляем его
                    _context.Entry(_currentOrder).State = EntityState.Modified;
                }

                // Сохраняем изменения в базе данных
                _context.SaveChanges();

                // Вызываем колбэк для обновления данных на странице заказов
                _onOrderUpdated?.Invoke();

                // Возвращаемся на страницу заказов
                NavigationService.GoBack();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Обрабатываем исключение оптимистичной блокировки
                var entry = ex.Entries.Single();
                var databaseValues = entry.GetDatabaseValues();

                if (databaseValues == null)
                {
                    // Заказ был удален другим пользователем
                    var result = MessageBox.Show("Заказ был удален другим пользователем. Хотите создать новый заказ?", "Ошибка", MessageBoxButton.YesNo, MessageBoxImage.Error);

                    if (result == MessageBoxResult.Yes)
                    {
                        
                        var newOrder = new order
                        {
                            product_id = _currentOrder.product_id,
                            count = _currentOrder.count,
                            price = _currentOrder.price,
                            sum = _currentOrder.sum,
                            date = DateTime.Now,
                            user_id = _currentOrder.user_id 
                        };

                        
                        newOrder.id = GenerateNewOrderId();

                        
                        _context.order.Add(newOrder);
                        

                       
                        _onOrderUpdated?.Invoke();

                        
                        NavigationService.GoBack();
                    }
                }
                else
                {
                    MessageBox.Show("Заказ был изменен другим пользователем. Пожалуйста, перезагрузите данные и попробуйте снова.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при сохранении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }






        private int GenerateNewOrderId()
        {
            // Генерируем ID на основе максимального значения из базы данных
            var maxOrderId = _context.order.Max(c => c.id);
            return maxOrderId + 1;
        }
    }
}