using System.Linq;
using System.Windows;
using ElectronicsStore.Models;
using ElectronicsStore.Services;

namespace ElectronicsStore
{
    public enum LookupType { Category, Brand, Tag }

    public partial class LookupForm : Window
    {
        private readonly LookupType _type;

        public LookupForm(LookupType type)
        {
            InitializeComponent();
            _type = type;

            HeaderText.Text = type switch
            {
                LookupType.Category => "Категории",
                LookupType.Brand => "Бренды",
                LookupType.Tag => "Теги",
                _ => "Справочник"
            };
            Title = HeaderText.Text;

            LoadItems();
        }

        private void LoadItems()
        {
            var ctx = DBService.Instance.Context;
            switch (_type)
            {
                case LookupType.Category:
                    ItemsList.ItemsSource = ctx.Categories.ToList();
                    break;
                case LookupType.Brand:
                    ItemsList.ItemsSource = ctx.Brands.ToList();
                    break;
                case LookupType.Tag:
                    ItemsList.ItemsSource = ctx.Tags.ToList();
                    break;
            }
        }

        private void AddItem(object sender, RoutedEventArgs e)
        {
            var editForm = new LookupEditForm($"Новая запись — {HeaderText.Text}") { Owner = this };
            if (editForm.ShowDialog() != true) return;

            var name = editForm.ItemName;
            var ctx = DBService.Instance.Context;

            // Проверка на дубликат
            bool exists = _type switch
            {
                LookupType.Category => ctx.Categories.Any(x => x.Name == name),
                LookupType.Brand => ctx.Brands.Any(x => x.Name == name),
                LookupType.Tag => ctx.Tags.Any(x => x.Name == name),
                _ => false
            };
            if (exists)
            {
                MessageBox.Show("Такое название уже есть", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int newId = _type switch
            {
                LookupType.Category => ctx.Categories.Any() ? ctx.Categories.Max(x => x.Id) + 1 : 1,
                LookupType.Brand => ctx.Brands.Any() ? ctx.Brands.Max(x => x.Id) + 1 : 1,
                LookupType.Tag => ctx.Tags.Any() ? ctx.Tags.Max(x => x.Id) + 1 : 1,
                _ => 1
            };

            switch (_type)
            {
                case LookupType.Category:
                    ctx.Categories.Add(new Category { Id = newId, Name = name });
                    break;
                case LookupType.Brand:
                    ctx.Brands.Add(new Brand { Id = newId, Name = name });
                    break;
                case LookupType.Tag:
                    ctx.Tags.Add(new Tag { Id = newId, Name = name });
                    break;
            }
            ctx.SaveChanges();
            LoadItems();
        }

        private void EditItem(object sender, RoutedEventArgs e)
        {
            if (ItemsList.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись в списке", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selected = ItemsList.SelectedItem;
            string oldName = selected.Name;
            int selectedId = (int)selected.Id;   

            var editForm = new LookupEditForm($"Изменение — {HeaderText.Text}", oldName) { Owner = this };
            if (editForm.ShowDialog() != true) return;

            var newName = editForm.ItemName;
            if (newName == oldName) return;

            var ctx = DBService.Instance.Context;

            bool exists = _type switch
            {
                LookupType.Category => ctx.Categories.Any(x => x.Name == newName && x.Id != selectedId),
                LookupType.Brand => ctx.Brands.Any(x => x.Name == newName && x.Id != selectedId),
                LookupType.Tag => ctx.Tags.Any(x => x.Name == newName && x.Id != selectedId),
                _ => false
            };
            if (exists)
            {
                MessageBox.Show("Такое название уже есть", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            selected.Name = newName;

            switch (_type)
            {
                case LookupType.Category:
                    ctx.Categories.Update((Category)selected);
                    break;
                case LookupType.Brand:
                    ctx.Brands.Update((Brand)selected);
                    break;
                case LookupType.Tag:
                    ctx.Tags.Update((Tag)selected);
                    break;
            }

            ctx.SaveChanges();
            LoadItems();
        }

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            if (ItemsList.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись в списке", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selected = ItemsList.SelectedItem;
            var result = MessageBox.Show($"Удалить «{selected.Name}»?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            var ctx = DBService.Instance.Context;
            try
            {
                switch (_type)
                {
                    case LookupType.Category:
                        ctx.Categories.Remove((Category)selected);
                        break;
                    case LookupType.Brand:
                        ctx.Brands.Remove((Brand)selected);
                        break;
                    case LookupType.Tag:
                        ctx.Tags.Remove((Tag)selected);
                        break;
                }
                ctx.SaveChanges();
                LoadItems();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Не удалось удалить: {ex.Message}\n\n" +
                    "на эту запись ссылаются товары",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}