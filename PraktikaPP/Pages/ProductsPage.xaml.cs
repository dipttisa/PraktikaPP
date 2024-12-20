using PraktikaPP.DopPages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
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
    /// Логика взаимодействия для ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        private ObservableCollection<prodact> Product { get; set; }
        private PractikaDB _context;
       

        public ProductsPage()
        {
            
            InitializeComponent();
            Product = new ObservableCollection<prodact>();

            // Затем привязываем к ListView
            ProductsListView.ItemsSource = Product;
            LoadCategories();
            LoadProducts();
        }

        // Загружаем категории для фильтра
        private void LoadCategories()
        {
            var categories = PractikaDB.GetContext().categ.ToList();
            categories.Insert(0, new categ { id = 0, category_name = "Все категории" });
            CategoryFilterComboBox.ItemsSource = categories;
            CategoryFilterComboBox.SelectedIndex = 0;
        }

        private void LoadProducts()
        {
            try
            {
                // Загрузка продуктов и связанных категорий через Include
                var products = PractikaDB.GetContext().prodact
                    .Include(p => p.categ) // Подключаем категорию для каждого продукта
                    .ToList();

                // Привязываем данные продуктов к ListView
                ProductsListView.ItemsSource = products;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продуктов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик для добавления нового продукта
        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            // Генерируем новый ID для продукта
           

            // Создаем новый объект продукта с автоматически сгенерированным ID
            var newProduct = new prodact
            {
                id = 0,
                name_prod = string.Empty, // Устанавливаем название по умолчанию (пустое)
                id_cat = 1 // Устанавливаем категорию по умолчанию (например, 1)
            };

            // Переходим на страницу редактирования продукта
            NavigationService.Navigate(new ProductEditPage(newProduct, OnProductUpdated));
        }

        // Редактирование выбранного продукта
        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранный продукт из списка
            var selectedProduct = ProductsListView.SelectedItem as prodact;
            if (selectedProduct != null)
            {
                // Переходим на страницу редактирования продукта
                NavigationService.Navigate(new ProductEditPage(selectedProduct, OnProductUpdated));
            }
            else
            {
                // Если продукт не выбран, выводим сообщение об ошибке
                MessageBox.Show("Выберите продукт для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
       

        // Обработчик изменения фильтра по категориям
        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        // Обработчик изменения текста в поиске
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        // Применяем фильтр на основе выбранной категории и текста поиска
        private void ApplyFilter()
        {
            try
            {
                var selectedCategory = CategoryFilterComboBox.SelectedItem as categ;
                var searchText = SearchTextBox.Text.Trim();

                // Запрос с динамическими фильтрами
                var query = PractikaDB.GetContext().prodact
                    .Include(p => p.categ)
                    .AsQueryable();

                // Фильтрация по категории
                if (selectedCategory != null && selectedCategory.id != 0)
                {
                    query = query.Where(p => p.id_cat == selectedCategory.id);
                }

                // Фильтрация по тексту поиска
                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(p => p.name_prod.Contains(searchText));
                }

                // Получаем результат и обновляем ListView
                var filteredProducts = query
                    .ToList(); // Выполняем запрос только здесь

                ProductsListView.ItemsSource = filteredProducts;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void OnProductUpdated()
        {
            LoadProducts();
        }

    }

}

