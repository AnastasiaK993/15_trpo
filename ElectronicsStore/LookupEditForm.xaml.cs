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
using System.Windows.Shapes;
using System.Windows;

namespace ElectronicsStore
{
    public partial class LookupEditForm : Window
    {
        public string ItemName { get; private set; } = "";

        public LookupEditForm(string title, string initialName = "")
        {
            InitializeComponent();
            Title = title;
            HeaderText.Text = title;
            NameBox.Text = initialName;
            NameBox.Focus();
            NameBox.SelectAll();
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            var name = NameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Введите название", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ItemName = name;
            DialogResult = true;
            Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}