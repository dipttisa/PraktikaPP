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
    /// Логика взаимодействия для ProductEditPage.xaml
    /// </summary>
    public partial class ProductEditPage : Page
    {

        private prodact _product;

        public ProductEditPage()
        {
            InitializeComponent();
            LoadCategories();
        }

        public ProductEditPage(prodact product) : this()
        {
            _product = product;
            ProductNameTextBox.Text = product.name_prod;
            CategoryComboBox.SelectedValue = product.id_cat;
        }

        private void LoadCategories()
        {
            var categories = PractikaDB.GetContext().categ.ToList();
            CategoryComboBox.ItemsSource = categories;
            CategoryComboBox.SelectedIndex = 0;
        }

        private void SaveProduct_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProductNameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            prodact product = _product ?? new prodact();

            product.name_prod = ProductNameTextBox.Text;
            product.id_cat = (int)CategoryComboBox.SelectedValue;

            // Если это новый продукт, не устанавливаем id вручную. Оно сгенерируется автоматически.
            if (_product != null)
            {
                // Не нужно вручную назначать id. Это произойдет автоматически при сохранении.
                PractikaDB.GetContext().prodact.Add(product);
               
            }
            else
            {
                // Если редактируем существующий продукт, просто сохраняем изменения
                PractikaDB.GetContext().Entry(product).State = System.Data.Entity.EntityState.Modified;
                
            }
                PractikaDB.GetContext().SaveChanges();
            NavigationService.GoBack();


        }
    }
}

