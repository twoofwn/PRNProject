using PRNProject.Models;
using System.Windows;
using System.Windows.Controls;

namespace PRNProject.Pages
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new MyTaskContext())
            {
                string username = UsernameTextBox.Text;
                string password = PasswordBox.Password;

                // Chỉ trả về nếu có đúng 1 phần tử thôi
                var user = db.Users
                             .SingleOrDefault(u => u.Username == username
                                                && u.PasswordHash == password);

                if (user != null)
                {
                    AppSession.CurrentUser = user;

                    this.NavigationService.Navigate(new HomePage(user.Username));
                }
                else
                {
                    MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!", "Lỗi",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    PasswordBox.Clear();
                }
            }
        }

        // Chuyển hướng sang trang đăng ký
        private void GoToRegisterButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new RegisterPage());
        }

    }
}
