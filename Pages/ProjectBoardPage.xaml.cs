using Microsoft.EntityFrameworkCore;
using PRNProject.Models;
using PRNProject.Windows; // Đảm bảo TaskDetailWindow nằm trong namespace này
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
    /// Interaction logic for ProjectBoardPage.xaml
    /// </summary>
    public partial class ProjectBoardPage : Page
    {
        private Project _currentProject;
        private readonly MyTaskContext _context = new MyTaskContext();
        private Point _startPoint;
        private bool _isDragging = false;

        public ProjectBoardPage(Project project)
        {
            InitializeComponent();
            _currentProject = project;
            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ProjectTitleTextBlock.Text = _currentProject.Title;
            LoadBoard();
        }

        private void LoadBoard()
        {
            if (_currentProject == null) return;
            var tasks = _context.Tasks
                .AsNoTracking()
                .Include(t => t.Priority)
                .Where(t => t.ProjectId == _currentProject.ProjectId)
                .ToList();

            TodoListView.ItemsSource = tasks.Where(t => t.StatusId == 1).ToList();
            InProgressListView.ItemsSource = tasks.Where(t => t.StatusId == 2).ToList();
            DoneListView.ItemsSource = tasks.Where(t => t.StatusId == 3).ToList();
        }

        #region Project CRUD
        private void EditProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new AddEditProjectWindow(_currentProject);

            if (editWindow.ShowDialog() == true)
            {
                _context.Entry(_currentProject).State = EntityState.Detached; // Vì trong addEditProjectWindow dùng 1 dbcontext khác nên phải dùng detached để quên nó đi -> cập nhật mới

                _currentProject = _context.Projects.Find(_currentProject.ProjectId);

                ProjectTitleTextBlock.Text = _currentProject.Title;
            }
        }

        private void DeleteProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa project '{_currentProject.Title}' và tất cả các task bên trong không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                _context.Projects.Remove(_currentProject);
                _context.SaveChanges();
                // Quay lại trang danh sách project
                if (NavigationService.CanGoBack) NavigationService.GoBack();
            }
        }
        #endregion

        #region Task CRUD
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditTaskWindow();
            if (addWindow.ShowDialog() == true)
            {
                var newTask = addWindow.Task;
                newTask.ProjectId = _currentProject.ProjectId;
                newTask.OwnerUserId = _currentProject.OwnerUserId; // Gán cho cùng owner
                _context.Tasks.Add(newTask);
                _context.SaveChanges();
                LoadBoard();
            }
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            var selectedTask = GetSelectedTask();
            if (selectedTask == null)
            {
                MessageBox.Show("Vui lòng chọn một task để sửa.");
                return;
            }
            var projectOwner = _context.Users.Find(_currentProject.OwnerUserId);
            if (projectOwner == null)
            {
                MessageBox.Show("Không tìm thấy người dùng của project này.");
                return;
            }

            // Giả định bạn có một cửa sổ tên là TaskDetailWindow để hiển thị chi tiết
            var detailWindow = new TaskDetailWindow(selectedTask, projectOwner);
            if (detailWindow.ShowDialog() == true)
            {
                LoadBoard(); // Tải lại bảng sau khi chỉnh sửa
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            var selectedTask = GetSelectedTask();
            if (selectedTask == null)
            {
                MessageBox.Show("Vui lòng chọn một task để xóa.");
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa task '{selectedTask.Title}' không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                _context.Tasks.Remove(selectedTask);
                _context.SaveChanges();
                LoadBoard();
            }
        }

        private Models.Task GetSelectedTask()
        {
            if (TodoListView.SelectedItem != null) return (Models.Task)TodoListView.SelectedItem;
            if (InProgressListView.SelectedItem != null) return (Models.Task)InProgressListView.SelectedItem;
            if (DoneListView.SelectedItem != null) return (Models.Task)DoneListView.SelectedItem;
            return null;
        }

        private void Task_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listView = sender as ListView;
            if (listView != null && listView.SelectedItem != null)
            {
                var selectedTask = listView.SelectedItem as Models.Task;
                if (selectedTask == null) return;

                // Lấy thông tin người dùng sở hữu project
                var projectOwner = _context.Users.Find(_currentProject.OwnerUserId);
                if (projectOwner == null)
                {
                    MessageBox.Show("Không tìm thấy người dùng của project này.");
                    return;
                }

                // Giả định bạn có một cửa sổ tên là TaskDetailWindow để hiển thị chi tiết
                var detailWindow = new TaskDetailWindow(selectedTask, projectOwner);
                if (detailWindow.ShowDialog() == true)
                {
                    LoadBoard(); // Tải lại bảng sau khi chỉnh sửa
                }
            }
        }

        #endregion

        #region Navigation
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
        #endregion

        #region Drag and Drop (Tái sử dụng từ HomePage)
        private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _isDragging = false;
        }

        private void ListView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)  // xem chuột trái có đang bấm và dragging = false ko
            {
                Point position = e.GetPosition(null);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)        // check vị trí ban đầu của chuột
                {
                    _isDragging = true;
                    var listView = sender as ListView;
                    var taskItem = FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);  // tìm phần tử cha của e.originalSouce (ptu thực sự mà chuột đang tương tác) 
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
                        LoadBoard();
                    }
                }
            }
            _isDragging = false;
        }

        // đệ quy tìm cấp cha, trả về theo giá trị của T
        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T) return (T)current;
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }
        #endregion
    }
}