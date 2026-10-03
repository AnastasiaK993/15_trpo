using ElectronicsStore.Models;
using ElectronicsStore.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ElectronicsStore
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public bool IsManager { get; set; }

        public ObservableCollection<Product> Products { get; set; } = new();
        public ICollectionView ProductsView { get; set; }

        private string _searchQuery = "";
        public string SearchQuery
        {
            get => _searchQuery;
            set { _searchQuery = value; OnPropertyChanged(nameof(SearchQuery)); }
        }

        private string _priceFrom = "";
        public string PriceFrom
        {
            get => _priceFrom;
            set { _priceFrom = value; OnPropertyChanged(nameof(PriceFrom)); }
        }

        private string _priceTo = "";
        public string PriceTo
        {
            get => _priceTo;
            set { _priceTo = value; OnPropertyChanged(nameof(PriceTo)); }
        }

        public int TotalCount => Products.Count;
        public int FilteredCount => ProductsView?.Cast<object>().Count() ?? 0;

        public MainWindow(bool isManager = false)
        {
            InitializeComponent();
            IsManager = isManager;

            Title = isManager
                ? "Магазин электроники — Менеджер"
                : "Магазин электроники — Посетитель";

            DataContext = this;

            LoadProducts();
            LoadFilters();

            ProductsView = CollectionViewSource.GetDefaultView(Products);
            ProductsView.Filter = FilterProducts;

            if (!IsManager)
            {
                ManagerButtons.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadProducts()
        {
            var ctx = DBService.Instance.Context;

            var products = ctx.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Tags)
                .ToList();

            Products.Clear();
            foreach (var p in products)
                Products.Add(p);
        }
        private void LoadFilters()
        {
            var ctx = DBService.Instance.Context;

            CategoryBox.ItemsSource = ctx.Categories.ToList();
            CategoryBox.DisplayMemberPath = "Name";
            CategoryBox.SelectedIndex = -1;

            BrandBox.ItemsSource = ctx.Brands.ToList();
            BrandBox.DisplayMemberPath = "Name";
            BrandBox.SelectedIndex = -1;
        }

        private bool FilterProducts(object obj)
        {
            if (obj is not Product p) return false;

            if (!string.IsNullOrWhiteSpace(SearchQuery) &&
                !p.Name.Contains(SearchQuery, System.StringComparison.CurrentCultureIgnoreCase))
                return false;

            if (CategoryBox.SelectedItem is Category cat && p.CategoryId != cat.Id)
                return false;

            if (BrandBox.SelectedItem is Brand br && p.BrandId != br.Id)
                return false;

            if (!string.IsNullOrWhiteSpace(PriceFrom) &&
                double.TryParse(PriceFrom, out var pf) && p.Price < pf)
                return false;

            if (!string.IsNullOrWhiteSpace(PriceTo) &&
                double.TryParse(PriceTo, out var pt) && p.Price > pt)
                return false;

            return true;
        }

        private void FilterChanged(object sender, RoutedEventArgs e)
        {
            ProductsView?.Refresh();
            OnPropertyChanged(nameof(FilteredCount));
        }

        private void ResetFilters(object sender, RoutedEventArgs e)
        {
            SearchQuery = "";
            PriceFrom = "";
            PriceTo = "";
            CategoryBox.SelectedIndex = -1;
            BrandBox.SelectedIndex = -1;
            SortBox.SelectedIndex = -1;

            ProductsView.SortDescriptions.Clear();
            ProductsView.Refresh();
            OnPropertyChanged(nameof(FilteredCount));
        }

        private void SortChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsView == null) return;
            ProductsView.SortDescriptions.Clear();

            if (SortBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                switch (tag)
                {
                    case "NameAsc":
                        ProductsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                        break;
                    case "PriceAsc":
                        ProductsView.SortDescriptions.Add(new SortDescription("Price", ListSortDirection.Ascending));
                        break;
                    case "PriceDesc":
                        ProductsView.SortDescriptions.Add(new SortDescription("Price", ListSortDirection.Descending));
                        break;
                    case "StockAsc":
                        ProductsView.SortDescriptions.Add(new SortDescription("Stock", ListSortDirection.Ascending));
                        break;
                    case "StockDesc":
                        ProductsView.SortDescriptions.Add(new SortDescription("Stock", ListSortDirection.Descending));
                        break;
                }
            }
            ProductsView.Refresh();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        private void AddProduct(object sender, RoutedEventArgs e)
        {
            var form = new ProductForm();
            if (form.ShowDialog() == true)
                LoadProducts();
        }

        private void EditProduct(object sender, RoutedEventArgs e)
        {
            if (ProductsList.SelectedItem is not Product p)
            {
                MessageBox.Show("Выберите товар в списке", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var form = new ProductForm(p);
            if (form.ShowDialog() == true)
                LoadProducts();
        }

        private void DeleteProduct(object sender, RoutedEventArgs e)
        {
            if (ProductsList.SelectedItem is not Product p)
            {
                MessageBox.Show("Выберите товар в списке", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить товар «{p.Name}»?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            var ctx = DBService.Instance.Context;
            ctx.Products.Remove(p);
            ctx.SaveChanges();

            LoadProducts();
        }

        private void OpenCategories(object sender, RoutedEventArgs e)
        {
            var form = new LookupForm(LookupType.Category) { Owner = this };
            form.ShowDialog();
            LoadProducts();
            LoadFilters();
        }

        private void OpenBrands(object sender, RoutedEventArgs e)
        {
            var form = new LookupForm(LookupType.Brand) { Owner = this };
            form.ShowDialog();
            LoadProducts();
            LoadFilters();
        }

        private void OpenTags(object sender, RoutedEventArgs e)
        {
            var form = new LookupForm(LookupType.Tag) { Owner = this };
            form.ShowDialog();
            LoadProducts();
        }

        private void ProductsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

}