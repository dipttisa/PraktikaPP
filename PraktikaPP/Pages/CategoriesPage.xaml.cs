using PraktikaPP.DopPages;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;

namespace PraktikaPP.Pages
{
    public partial class CategoriesPage : Page
    {
        private ObservableCollection<categ> Categories { get; set; }

        public CategoriesPage()
        {
            InitializeComponent();
            Categories = new ObservableCollection<categ>();

            // Затем привязываем к ListView
            CategoriesListView.ItemsSource = Categories;

            // Наконец, загружаем данные
            LoadCategories();
        }

        // Загрузка категорий из БД
        private void LoadCategories()
        {
            try
            {
                Categories.Clear();
                var categories = PractikaDB.GetContext().categ.ToList();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Добавление новой категории
        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            var newCategory = new categ
            {
                id = GetNextCategoryId(),
                category_name = string.Empty
            };

            NavigationService.Navigate(new CategoryEditPage(newCategory, OnCategoryUpdated));
        }

        // Редактирование выбранной категории
        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            var selectedCategory = CategoriesListView.SelectedItem as categ;
            if (selectedCategory != null)
            {
                NavigationService.Navigate(new CategoryEditPage(selectedCategory, OnCategoryUpdated));
            }
            else
            {
                MessageBox.Show("Выберите категорию для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Фильтрация категорий
        private void SearchCategoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            try
            {
                var searchText = SearchCategoryTextBox.Text.Trim();

                if (string.IsNullOrEmpty(searchText))
                {
                    // Если поисковый запрос пуст, возвращаем полный список
                    CategoriesListView.ItemsSource = Categories;
                    return;
                }

                // Фильтрация по тексту поиска
                var filteredCategories = Categories
                    .Where(c => c.category_name.Contains(searchText))
                    .ToList();

                CategoriesListView.ItemsSource = filteredCategories;
            }
            catch (Exception ex)
            {
            }
        }

        // Получение следующего ID
        private int GetNextCategoryId()
        {
            return Categories.Any() ? Categories.Max(c => c.id) + 1 : 1;
        }

        // Колбэк для обновления данных после редактирования
        private void OnCategoryUpdated()
        {
            LoadCategories();
        }
    }
}