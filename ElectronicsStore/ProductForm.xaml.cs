using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ElectronicsStore.Models;
using ElectronicsStore.Services;

namespace ElectronicsStore
{
    public partial class ProductForm : Window
    {
        private readonly Product _product;
        private readonly bool _isEdit;
        private readonly HashSet<int> _selectedTagIds = new();

        public ProductForm(Product product = null)
        {
            InitializeComponent();

            LoadCategoriesAndBrands();
            LoadTags();

            if (product != null)
            {
                _product = product;
                _isEdit = true;
                HeaderText.Text = "Редактирование товара";

                NameBox.Text = product.Name;
                DescriptionBox.Text = product.Description;
                PriceBox.Text = product.Price.ToString();
                StockBox.Text = product.Stock.ToString();
                RatingBox.Text = product.Rating.ToString();
                CreatedAtPicker.SelectedDate = product.CreatedAt.ToDateTime(TimeOnly.MinValue);

                CategoryBox.SelectedValue = product.CategoryId;
                BrandBox.SelectedValue = product.BrandId;

                foreach (var tag in product.Tags)
                    _selectedTagIds.Add(tag.Id);
                ApplyTagSelection();
            }
            else
            {
                _product = new Product();
                _isEdit = false;
                HeaderText.Text = "Добавление товара";
                CreatedAtPicker.SelectedDate = DateTime.Today;
                CategoryBox.SelectedIndex = 0;
                BrandBox.SelectedIndex = 0;
            }
        }

        private void LoadCategoriesAndBrands()
        {
            var ctx = DBService.Instance.Context;
            CategoryBox.ItemsSource = ctx.Categories.ToList();
            BrandBox.ItemsSource = ctx.Brands.ToList();
        }

        private void LoadTags()
        {
            var ctx = DBService.Instance.Context;
            TagsList.ItemsSource = ctx.Tags.ToList();
        }

        private void ApplyTagSelection()
        {
            if (TagsList.ItemsSource == null) return;
            foreach (var item in TagsList.Items)
            {
                if (item is Tag tag && TagsList.ItemContainerGenerator.ContainerFromItem(item) is ContentPresenter cp)
                {
                    var cb = FindCheckBox(cp);
                    if (cb != null) cb.IsChecked = _selectedTagIds.Contains(tag.Id);
                }
            }
        }

        private CheckBox FindCheckBox(DependencyObject parent)
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is CheckBox cb) return cb;
                var result = FindCheckBox(child);
                if (result != null) return result;
            }
            return null;
        }

        private void TagChecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is int id)
            {
                if (cb.IsChecked == true) _selectedTagIds.Add(id);
                else _selectedTagIds.Remove(id);
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Введите название товара", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(PriceBox.Text, out var price) || price < 0)
            {
                MessageBox.Show("Введите корректную цену", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(StockBox.Text, out var stock) || stock < 0)
            {
                MessageBox.Show("Введите корректный остаток", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(RatingBox.Text, out var rating) || rating < 0 || rating > 5)
            {
                MessageBox.Show("Рейтинг должен быть от 0 до 5", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CreatedAtPicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату создания", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CategoryBox.SelectedItem is not Category cat)
            {
                MessageBox.Show("Выберите категорию", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (BrandBox.SelectedItem is not Brand brand)
            {
                MessageBox.Show("Выберите бренд", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ctx = DBService.Instance.Context;

            _product.Name = NameBox.Text.Trim();
            _product.Description = string.IsNullOrWhiteSpace(DescriptionBox.Text)
                ? null
                : DescriptionBox.Text.Trim();
            _product.Price = price;
            _product.Stock = stock;
            _product.Rating = rating;
            _product.CreatedAt = DateOnly.FromDateTime(CreatedAtPicker.SelectedDate.Value);
            _product.CategoryId = cat.Id;
            _product.BrandId = brand.Id;

            var selectedTags = ctx.Tags
                .Where(t => _selectedTagIds.Contains(t.Id))
                .ToList();

            _product.Tags.Clear();
            foreach (var tag in selectedTags)
                _product.Tags.Add(tag);

            if (_isEdit)
            {
                ctx.Products.Update(_product);
            }
            else
            {
                int maxId = ctx.Products.Any() ? ctx.Products.Max(p => p.Id) : 0;
                _product.Id = maxId + 1;
                ctx.Products.Add(_product);
            }

            try
            {
                ctx.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}