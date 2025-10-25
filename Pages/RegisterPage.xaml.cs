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
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string displayName = DisplayNameTextBox.Text;
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // --- Validation ---
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Tên đăng nhập và mật khẩu không được để trống!", "Lỗi",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Mật khẩu và xác nhận mật khẩu không khớp!", "Lỗi",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new MyTaskContext())
                {
                    // Kiểm tra xem username đã tồn tại chưa
                    var existingUser = db.Users.SingleOrDefault(u => u.Username == username);
                    if (existingUser != null)
                    {
                        MessageBox.Show("Tên đăng nhập này đã tồn tại. Vui lòng chọn tên khác.", "Lỗi",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Tạo user mới
                    var newUser = new User
                    {
                        Username = username,
                        PasswordHash = password, 
                        DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName,
                        Email = string.IsNullOrWhiteSpace(email) ? null : email,
                        CreatedAt = DateTime.UtcNow 
                    };

                    db.Users.Add(newUser);
                    db.SaveChanges();

                    MessageBox.Show("Đăng ký tài khoản thành công! Vui lòng đăng nhập.", "Thành công",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                    // Điều hướng về trang đăng nhập
                    GoToLoginButton_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi trong quá trình đăng ký: " + ex.Message, "Lỗi hệ thống",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GoToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
            else
            {
                this.NavigationService.Navigate(new LoginPage());
            }
        }
    }
}
