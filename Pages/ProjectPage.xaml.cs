using Microsoft.EntityFrameworkCore;
using PRNProject.Models;
using PRNProject.Windows;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace PRNProject.Pages
{
    public partial class ProjectPage : Page
    {
        private readonly MyTaskContext _context = new MyTaskContext();
        private readonly User _currentUser;

        public ProjectPage(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProjects();
        }

        private void LoadProjects()
        {
            if (_currentUser == null) return;

            var projects = _context.Projects
                .AsNoTracking()
                .Include(p => p.OwnerUser)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.User)
                .Where(p => p.OwnerUserId == _currentUser.UserId || p.ProjectMembers.Any(pm => pm.UserId == _currentUser.UserId))
                .ToList();

            ProjectsItemsControl.ItemsSource = projects;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditProjectWindow();
            if (addWindow.ShowDialog() == true)
            {
                var newProject = addWindow.Project;
                newProject.OwnerUserId = _currentUser.UserId;
                _context.Projects.Add(newProject);
                _context.SaveChanges();
                LoadProjects();
            }
        }

        private void ProjectCard_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Project selectedProject)
            {
                this.NavigationService.Navigate(new ProjectBoardPage(selectedProject));
            }
        }
    }

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
                return new SolidColorBrush(Colors.Gray);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}