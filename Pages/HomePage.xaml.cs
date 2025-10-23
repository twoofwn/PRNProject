using Microsoft.EntityFrameworkCore;
using PRNProject.Models;
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
    public partial class HomePage : Page
    {
        private readonly MyTaskContext _context = new MyTaskContext();
        private User _currentUser;
        private Point _startPoint;
        private bool _isDragging = false;

        public HomePage(string loggedInUsername)
        {
            InitializeComponent();
            WelcomeTextBlock.Text = $"Chào mừng, {loggedInUsername}!";
            _currentUser = _context.Users.FirstOrDefault(u => u.Username == loggedInUsername);
            SidebarControl.MenuClicked += SidebarControl_MenuClicked;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Không tìm thấy người dùng. Vui lòng đăng nhập lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                // Có thể điều hướng về trang đăng nhập ở đây
                return;
            }
            LoadFilters();
            LoadTasks();
        }

        private void LoadFilters()
        {
            // Load Priority Filter
            var priorities = _context.Priorities.ToList();
            priorities.Insert(0, new Priority { PriorityId = 0, Name = "Tất cả độ ưu tiên" });
            PriorityFilterComboBox.ItemsSource = priorities;
            PriorityFilterComboBox.DisplayMemberPath = "Name";
            PriorityFilterComboBox.SelectedValuePath = "PriorityId";
            PriorityFilterComboBox.SelectedIndex = 0;

            // Load Tag Filter
            var tags = _context.Tags.Where(t => t.OwnerUserId == _currentUser.UserId).ToList();
            tags.Insert(0, new Tag { TagId = 0, Name = "Tất cả nhãn" });
            TagFilterComboBox.ItemsSource = tags;
            TagFilterComboBox.DisplayMemberPath = "Name";
            TagFilterComboBox.SelectedValuePath = "TagId";
            TagFilterComboBox.SelectedIndex = 0;
        }

        private void LoadTasks()
        {
            if (_currentUser == null) return;

            var query = _context.Tasks
                .Include(t => t.Priority)
                .Include(t => t.Tags)
                .Where(t => t.OwnerUserId == _currentUser.UserId);

            // Apply search filter
            string searchText = SearchTextBox.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(t => t.Title.ToLower().Contains(searchText));
            }

            // Apply priority filter
            if (PriorityFilterComboBox.SelectedValue is int priorityId && priorityId > 0)
            {
                query = query.Where(t => t.PriorityId == priorityId);
            }

            // Apply tag filter
            if (TagFilterComboBox.SelectedValue is int tagId && tagId > 0)
            {
                query = query.Where(t => t.Tags.Any(tt => tt.TagId == tagId));
            }

            var tasks = query.ToList();

            TodoListView.ItemsSource = tasks.Where(t => t.StatusId == 1).ToList();
            InProgressListView.ItemsSource = tasks.Where(t => t.StatusId == 2).ToList();
            DoneListView.ItemsSource = tasks.Where(t => t.StatusId == 3).ToList();
        }

        // Event Handlers for Filters
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadTasks();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ensure the event handler is not called during initialization
            if (IsLoaded)
            {
                LoadTasks();
            }
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            PriorityFilterComboBox.SelectedIndex = 0;
            TagFilterComboBox.SelectedIndex = 0;
            LoadTasks();
        }

        // Drag and Drop Logic
        private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _isDragging = false;
        }

        private void ListView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
            {
                Point position = e.GetPosition(null);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    _isDragging = true;
                    var listView = sender as ListView;
                    var taskItem = FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);

                    if (taskItem != null)
                    {
                        var task = (Models.Task)listView.ItemContainerGenerator.ItemFromContainer(taskItem);
                        DataObject dragData = new DataObject("myTaskFormat", task);
                        DragDrop.DoDragDrop(taskItem, dragData, DragDropEffects.Move);
                    }
                }
            }
        }

        private void ListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("myTaskFormat"))
            {
                var droppedTask = e.Data.GetData("myTaskFormat") as Models.Task;
                var targetListView = sender as ListView;
                int newStatusId = 0;

                if (targetListView == TodoListView) newStatusId = 1;
                else if (targetListView == InProgressListView) newStatusId = 2;
                else if (targetListView == DoneListView) newStatusId = 3;

                if (droppedTask != null && newStatusId != 0 && droppedTask.StatusId != newStatusId)
                {
                    var taskInDb = _context.Tasks.Find(droppedTask.TaskId);
                    if (taskInDb != null)
                    {
                        taskInDb.StatusId = newStatusId;
                        _context.SaveChanges();
                        LoadTasks(); // Refresh the board
                    }
                }
            }
            _isDragging = false;
        }

        // Helper to find parent control in visual tree
        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        // Navigation to Task Details Page
        private void Task_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listView = sender as ListView;
            if (listView.SelectedItem is Models.Task selectedTask)
            {
                // Giả sử bạn có một trang tên là TaskPage nhận TaskId làm tham số
                // var taskPage = new TaskPage(selectedTask.TaskId);
                // this.NavigationService.Navigate(taskPage);

                MessageBox.Show($"Mở chi tiết cho công việc: '{selectedTask.Title}' (ID: {selectedTask.TaskId})");
            }
        }

        private void SidebarControl_MenuClicked(object sender, string page)
        {
            // Handle sidebar navigation
        }
    }

    public class PriorityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int priorityId)
            {
                switch (priorityId)
                {
                    case 1: return new SolidColorBrush(Colors.Green); // Low
                    case 2: return new SolidColorBrush(Colors.DodgerBlue); // Normal
                    case 3: return new SolidColorBrush(Colors.Orange); // High
                    case 4: return new SolidColorBrush(Colors.Red); // Urgent
                    default: return new SolidColorBrush(Colors.Gray);
                }
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}