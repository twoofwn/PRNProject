using Microsoft.EntityFrameworkCore;
using PRNProject.Models;
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

        // Hàm mới để xử lý sự kiện click nút đăng ký
        private void GoToRegisterButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new RegisterPage());
        }

    }
}
