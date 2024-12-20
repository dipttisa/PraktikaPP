using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PraktikaPP.DopPages
{
    public partial class ProductEditPage : Page
    {
        private readonly PractikaDB _context;
        private readonly prodact _product;
        private readonly Action _onProductUpdated;

        public ProductEditPage(prodact product, Action onProductUpdated)
        {
            InitializeComponent();

            _context = PractikaDB.GetContext();
            _onProductUpdated = onProductUpdated;

            // Загружаем категории в ComboBox
            var categories = _context.categ.ToList();
            categories.Insert(0, new categ
            {
                id = 0,
                category_name = "Все категории"
            });
            CategoryComboBox.ItemsSource = categories;
            CategoryComboBox.DisplayMemberPath = "category_name";
            CategoryComboBox.SelectedValuePath = "id";

            // Инициализируем продукт
            _product = product ?? new prodact();
            ProductNameTextBox.Text = _product.name_prod;
            CategoryComboBox.SelectedValue = _product.id_cat;
        }

        private void SaveProduct_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProductNameTextBox.Text) ||CategoryComboBox.SelectedValue == null|| CategoryComboBox.SelectedValue.ToString() == "0")
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Заполняем данные продукта из формы
            _product.name_prod = ProductNameTextBox.Text.Trim();

            // Присваиваем значение category_id
            if (CategoryComboBox.SelectedValue is int selectedCategoryId)
            {
                _product.id_cat = selectedCategoryId; // Теперь category_id имеет тип int
            }
            else
            {
                // Обработка ошибки, если SelectedValue не является int
                MessageBox.Show("Выбранная категория имеет недопустимый тип.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Если это новый продукт (id == 0), вычисляем новый ID
                if (_product.id == 0)
                {
                    int maxId = _context.prodact.Any() ? _context.prodact.Max(p => p.id) : 0;
                    _product.id = maxId + 1;
                    _context.prodact.Add(_product);
                }
                else
                {
                    // Если это существующий продукт, обновляем его данные
                    var existingProduct = _context.prodact.Find(_product.id);
                    if (existingProduct != null)
                    {
                        existingProduct.name_prod = _product.name_prod;
                        existingProduct.id_cat = _product.id_cat;
                    }
                }

                // Сохраняем изменения в базе данных
                _context.SaveChanges();

                // Выводим сообщение об успешном сохранении
                MessageBox.Show("Продукт успешно сохранен!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                // Возвращаемся на предыдущую страницу
                NavigationService.GoBack();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Произошла ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        
        }
    }
}