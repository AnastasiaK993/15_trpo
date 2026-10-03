using System.Windows;

namespace ElectronicsStore
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginAsManager(object sender, RoutedEventArgs e)
        {
            if (PinBox.Password == "1234")
            {
                OpenMainWindow(isManager: true);
            }
            else
            {
                MessageBox.Show("неправильный пин-код",
                                "Ошибка входа",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void LoginAsVisitor(object sender, RoutedEventArgs e)
        {
            OpenMainWindow(isManager: false);
        }

        private void OpenMainWindow(bool isManager)
        {
            var main = new MainWindow(isManager);
            main.Show();
            this.Close();
        }
    }
}