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

namespace PRNProject.Pages.Components
{
    /// <summary>
    /// Interaction logic for Sidebar.xaml
    /// </summary>
    public partial class Sidebar : UserControl
    {

        public Sidebar()
        {
            InitializeComponent();
        }

        private void SidebarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton && clickedButton.Tag is string pageTag)
            {
                var currentUser = AppSession.CurrentUser;
                if (currentUser == null)
                {
                    MessageBox.Show("Lỗi: Không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại.", "Lỗi Session", MessageBoxButton.OK, MessageBoxImage.Warning);
                    MainWindow.MainNavigationFrame.Navigate(new LoginPage());
                    return;
                }

                switch (pageTag)
                {
                    case "Dashboard":
                        if (!(MainWindow.MainNavigationFrame.Content is HomePage))
                        {
                            MainWindow.MainNavigationFrame.Navigate(new HomePage(currentUser.Username));
                        }
                        break;
                    case "Projects":
                        if (!(MainWindow.MainNavigationFrame.Content is ProjectPage))
                        {
                            MainWindow.MainNavigationFrame.Navigate(new ProjectPage(currentUser));
                        }
                        break;
                    case "Tasks":
                        MessageBox.Show("Chức năng 'Tasks' chưa được triển khai.");
                        break;
                }
            }
        }
    }
}
