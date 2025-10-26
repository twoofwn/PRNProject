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
            // Tải thông tin người dùng vào các ô text
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
                // Tìm người dùng trong database
                var userInDb = _context.Users.FirstOrDefault(u => u.UserId == _currentUser.UserId);
                if (userInDb == null)
                {
                    ShowMessage("Error: Could not find user in database.", true);
                    return;
                }

                // Cập nhật thông tin
                userInDb.DisplayName = txtDisplayName.Text;
                userInDb.Email = txtEmail.Text;

                _context.SaveChanges();

                // Cập nhật thông tin trong AppSession
                AppSession.CurrentUser.DisplayName = userInDb.DisplayName;
                AppSession.CurrentUser.Email = userInDb.Email;

                ShowMessage("Information updated successfully!", false);
            }
            catch (Exception ex)
            {
                ShowMessage($"An error occurred: {ex.Message}", true);
            }
        }

        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string oldPassword = txtOldPassword.Password;
            string newPassword = txtNewPassword.Password;
            string confirmPassword = txtConfirmNewPassword.Password;

            // --- Validation ---
            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ShowMessage("Please fill in all password fields.", true);
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowMessage("New password and confirmation password do not match.", true);
                return;
            }
            if (_currentUser.PasswordHash != oldPassword)
            {
                ShowMessage("Old password is not correct.", true);
                return;
            }

            // --- Update Password ---
            try
            {
                var userInDb = _context.Users.FirstOrDefault(u => u.UserId == _currentUser.UserId);
                if (userInDb == null)
                {
                    ShowMessage("Error: Could not find user in database.", true);
                    return;
                }

                userInDb.PasswordHash = newPassword;
                _context.SaveChanges();

                AppSession.CurrentUser.PasswordHash = newPassword;

                txtOldPassword.Password = "";
                txtNewPassword.Password = "";
                txtConfirmNewPassword.Password = "";

                ShowMessage("Password changed successfully!", false);
            }
            catch (Exception ex)
            {
                ShowMessage($"An error occurred: {ex.Message}", true);
            }
        }

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