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

namespace PraktikaPP.DopPages
{
    /// <summary>
    /// Логика взаимодействия для CategoryEditPage.xaml
    /// </summary>
    public partial class CategoryEditPage : Page
    {
        private readonly categ _category;
        private readonly Action _onCategoryUpdated;

        public CategoryEditPage(categ category, Action onCategoryUpdated)
        {
            InitializeComponent();
            _category = category;
            _onCategoryUpdated = onCategoryUpdated;

            // Привязка данных
            CategoryNameTextBox.Text = _category.category_name;
        }

        private void SaveCategory_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CategoryNameTextBox.Text))
            {
                MessageBox.Show("Название категории не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Обновляем категорию
                _category.category_name = CategoryNameTextBox.Text;

                var context = PractikaDB.GetContext();
                if (!context.categ.Any(c => c.id == _category.id))
                {
                    context.categ.Add(_category); // Добавляем, если это новая категория
                }

                context.SaveChanges();

                MessageBox.Show("Категория сохранена.", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                // Обновляем список категорий
                _onCategoryUpdated?.Invoke();

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}