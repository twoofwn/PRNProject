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
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private readonly MyTaskContext _context = new MyTaskContext();
        public HomePage(string loggedInUsername)
        {
            InitializeComponent();
            WelcomeTextBlock.Text = $"Chào mừng, {loggedInUsername}!";
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var tasks = _context.Tasks
                                    .Include(t => t.Status)
                                    .Include(t => t.Priority)
                                    .ToList(); 

                TasksListView.ItemsSource = tasks;

                var projects = _context.Projects
                                       .Where(p => p.OwnerUserId == 1) 
                                       .ToList();
                ProjectsListView.ItemsSource = projects;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
