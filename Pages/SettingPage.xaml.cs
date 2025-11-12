using PRNProject.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PRNProject.Pages
{
    public partial class SettingPage : Page
    {
        private readonly MyTaskContext _context;
        private readonly User _currentUser;

        public SettingPage(User currentUser)
        {
            InitializeComponent();
            _context = new MyTaskContext();
            _currentUser = currentUser;

            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            //fill thông tin vào các ô input
            if (_currentUser != null)
            {
                txtDisplayName.Text = _currentUser.DisplayName;
                txtEmail.Text = _currentUser.Email;
            }
        }

        private void btnSaveInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtDisplayName.Text))
                {
                    ShowMessage("Tên hiển thị không được để trống.", true);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    ShowMessage("Email không được để trống.", true);
                    return;
                }
                // tìm trong database
                var userInDb = _context.Users.FirstOrDefault(u => u.UserId == _currentUser.UserId);
                if (userInDb == null)
                {
                    ShowMessage("Lỗi: Không tìm thấy người dùng trong cơ sở dữ liệu.", true);
                    return;
                }

                //cập nhật thông tin
                userInDb.DisplayName = txtDisplayName.Text;
                userInDb.Email = txtEmail.Text;

                _context.SaveChanges();

                //cập nhật trong appsession
                AppSession.CurrentUser.DisplayName = userInDb.DisplayName;
                AppSession.CurrentUser.Email = userInDb.Email;

                ShowMessage("Cập nhật thông tin thành công!", false);
            }
            catch (Exception ex)
            {
                ShowMessage($"Đã xảy ra lỗi: {ex.Message}", true);
            }
        }

        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string oldPassword = txtOldPassword.Password;
            string newPassword = txtNewPassword.Password;
            string confirmPassword = txtConfirmNewPassword.Password;

            //validation
            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ShowMessage("Vui lòng điền đầy đủ các trường mật khẩu.", true);
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowMessage("Mật khẩu mới và mật khẩu xác nhận không trùng khớp.", true);
                return;
            }
            if (_currentUser.PasswordHash != oldPassword)
            {
                ShowMessage("Mật khẩu cũ không chính xác.", true);
                return;
            }

            //update pass
            try
            {
                var userInDb = _context.Users.FirstOrDefault(u => u.UserId == _currentUser.UserId);
                if (userInDb == null)
                {
                    ShowMessage("Lỗi: Không tìm thấy người dùng trong cơ sở dữ liệu.", true);
                    return;
                }

                userInDb.PasswordHash = newPassword;
                _context.SaveChanges();

                AppSession.CurrentUser.PasswordHash = newPassword;

                txtOldPassword.Password = "";
                txtNewPassword.Password = "";
                txtConfirmNewPassword.Password = "";

                ShowMessage("Đổi mật khẩu thành công!", false);
            }
            catch (Exception ex)
            {
                ShowMessage($"Đã xảy ra lỗi: {ex.Message}", true);
            }
        }

        //hàm utils để show message
        private void ShowMessage(string message, bool isError)
        {
            txtMessage.Text = message;
            txtMessage.Foreground = isError ? Brushes.Red : Brushes.Green;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (!(MainWindow.MainNavigationFrame.Content is HomePage))
            {
                MainWindow.MainNavigationFrame.Navigate(new HomePage(_currentUser.Username));
            }
        }
    }
}
