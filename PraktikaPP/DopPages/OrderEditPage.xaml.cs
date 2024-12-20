using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PraktikaPP.DopPages
{
    public partial class OrderEditPage : Page
    {
        private int _userId; // ID текущего пользователя
        private order _order; // Текущий заказ для редактирования или добавления
        private PractikaDB _context;

        public OrderEditPage(order selectedOrder = null, int userId = 0)
        {
            InitializeComponent();

            // Проверяем, передан ли ID пользователя
            if (userId == 0)
            {
                MessageBox.Show("Ошибка: ID пользователя не передан!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService.GoBack();
                return;
            }

            _userId = userId; // Сохраняем ID пользователя

            // Если selectedOrder == null, создаем новый объект
            _order = selectedOrder ?? new order();

            // Устанавливаем ID авторизованного пользователя
            _order.user_id = userId;
            _context = PractikaDB.GetContext();

            LoadCategories();
            LoadProducts();

            // Устанавливаем начальное значение для цены
            PriceTextBox.Text = _order.price.ToString();

            // Подписываемся на событие изменения выбранной категории
            CategoryComboBox.SelectionChanged += CategoryComboBox_SelectionChanged;

            LoadOrderData();
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

        private void KolvotNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            CalculateTotal();
        }

        private void PriceTextBox_LostFocus(object sender, RoutedEventArgs e)
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

            if (!int.TryParse(KolvotNameTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Количество должно быть положительным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Цена заказа не указана или не является корректным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Обновляем текущий заказ с новыми значениями
            _order.product_id = (int)ProductComboBox.SelectedValue;
            _order.count = quantity;
            _order.price = price;  // Цена берется из текстового поля
            _order.sum = price * quantity; // Сумма вычисляется на основе цены и количества
            _order.date = DateTime.Now; // Обновляем дату
            _order.user_id = _userId; // Устанавливаем ID авторизованного пользователя

            try
            {
                // Если заказ новый, добавляем его в базу данных
                if (_order.id == 0)
                {
                    // Генерируем новый ID
                    int maxId = GetMaxOrderId();
                    _order.id = maxId + 1;

                    // Добавляем новый заказ в базу
                    _context.order.Add(_order);
                    _context.SaveChanges();
                }
                else
                {
                    // Если заказ существует, обновляем его
                    var existingOrder = _context.order.Find(_order.id);
                    if (existingOrder != null)
                    {
                        existingOrder.product_id = _order.product_id;
                        existingOrder.count = _order.count;
                        existingOrder.price = _order.price;
                        existingOrder.sum = _order.sum;
                        existingOrder.date = _order.date;
                        existingOrder.user_id = _order.user_id;
                    }
                    
                }

                // Сохраняем изменения в базе данных
               

                // Выводим сообщение об успешном сохранении
                MessageBox.Show("Заказ успешно сохранен!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                _context.SaveChanges();

                // Возвращаемся на предыдущую страницу
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
                        // Создаем новый заказ
                        var newOrder = new order
                        {
                            product_id = _order.product_id,
                            count = _order.count,
                            price = _order.price,
                            sum = _order.sum,
                            date = DateTime.Now,
                            user_id = _order.user_id
                        };

                        // Генерируем новый ID
                        int maxId = GetMaxOrderId();
                        newOrder.id = maxId + 1;

                        // Добавляем новый заказ в базу
                        _context.order.Add(newOrder);
                        _context.SaveChanges();

                        // Возвращаемся на предыдущую страницу
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

        // Метод для заполнения полей данными из выбранного заказа
        private void LoadOrderData()
        {
            ProductComboBox.Text = _order.product_id.ToString();
            PriceTextBox.Text = _order.price.ToString();
            KolvotNameTextBox.Text = _order.count.ToString();
            SumNameTextBox.Text = _order.sum.ToString();

            // Заполняем имя продукта, если product_id уже заполнен
            if (_order.product_id > 0)
            {
                _order.prodact.name_prod = GetProductNameById(_order.product_id);
                ProductComboBox.Text = _order.prodact.name_prod;
            }
        }

        // Обработчик нажатия кнопки "Отмена"
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Возвращаемся на предыдущую страницу
            NavigationService.GoBack();
        }

        // Метод для получения имени продукта по product_id
        private string GetProductNameById(int productId)
        {
            var product = _context.prodact.FirstOrDefault(p => p.id == productId);
            return product?.name_prod ?? "Unknown Product";
        }

        
        private int GetMaxOrderId()
        {
            var maxOrderId = _context.order.Max(c => c.id);
            return maxOrderId + 1;
        }
    }
}