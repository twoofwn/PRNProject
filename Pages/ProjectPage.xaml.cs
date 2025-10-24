using Microsoft.EntityFrameworkCore;
using PRNProject.Models;
using PRNProject.Windows;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for ProjectPage.xaml
    /// </summary>
    public partial class ProjectPage : Page
    {
        private readonly MyTaskContext _context = new MyTaskContext();
        private readonly User _currentUser;

        public ProjectPage(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            this.Loaded += Page_Loaded; // Load data when page is ready
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProjects();
        }

        private void LoadProjects()
        {
            if (_currentUser == null) return;
            ProjectsItemsControl.ItemsSource = _context.Projects
                                                  .Where(p => p.OwnerUserId == _currentUser.UserId)
                                                  .ToList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Mở cửa sổ thêm Project
            var addWindow = new AddEditProjectWindow();
            if (addWindow.ShowDialog() == true)
            {
                var newProject = addWindow.Project;
                newProject.OwnerUserId = _currentUser.UserId;
                _context.Projects.Add(newProject);
                _context.SaveChanges();
                LoadProjects(); // Tải lại danh sách
            }
        }

        // Sự kiện khi click vào một thẻ project
        private void ProjectCard_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Project selectedProject)
            {
                // Điều hướng đến trang chi tiết project
                this.NavigationService.Navigate(new ProjectBoardPage(selectedProject));
            }
        }
    }

    // Converter để chuyển mã màu Hex thành Brush
    public class HexToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string hex = value as string;
            if (string.IsNullOrWhiteSpace(hex))
            {
                return new SolidColorBrush(Colors.Transparent);
            }

            try
            {
                return (SolidColorBrush)(new BrushConverter().ConvertFrom(hex));
            }
            catch
            {
                return new SolidColorBrush(Colors.Gray); // Màu mặc định nếu mã hex lỗi
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
