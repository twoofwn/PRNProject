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
using System.Windows.Shapes;

namespace PRNProject.Windows
{
    /// <summary>
    /// Interaction logic for AddEditTaskWindow.xaml
    /// </summary>
    public partial class AddEditTaskWindow : Window
    {
        public Models.Task Task { get; private set; }
        private readonly MyTaskContext _context = new MyTaskContext();
        private readonly Models.User _currentUser;
        private readonly int? _projectId;

        public AddEditTaskWindow(Models.User currentUser, int? projectId = null, Models.Task task = null)
        {
            InitializeComponent();

            _currentUser = currentUser;
            _projectId = projectId;

            LoadComboBoxes(); // Hàm này giờ sẽ gọi cả LoadAssignees

            Task = task ?? new Models.Task();
            DueDatePicker.DisplayDateStart = DateTime.Today;
            if (task != null)
            {
                TitleTextBox.Text = Task.Title;
                DescriptionTextBox.Text = Task.Description;
                PriorityComboBox.SelectedValue = Task.PriorityId;
                StatusComboBox.SelectedValue = Task.StatusId;
                DueDatePicker.SelectedDate = Task.DueAt;
                AssignUserComboBox.SelectedValue = Task.AssignedUserId; 
                
            }
            LoadAssignees();
        }

        private void LoadAssignees()
        {
            var list = new List<User>();
            list.Add(new User
            {
                UserId = 0,
                DisplayName = "-- Chưa gán --"
            });

            if (_projectId == null)
            {
                list.Add(_currentUser);
            }
            else
            {
                var members = _context.ProjectMembers
                    .Include(pm => pm.User)
                    .Where(pm => pm.ProjectId == _projectId)
                    .Select(pm => pm.User)
                    .ToList();

                list.AddRange(members);
            }

            AssignUserComboBox.ItemsSource = list;
            AssignUserComboBox.DisplayMemberPath = "DisplayName";
            AssignUserComboBox.SelectedValuePath = "UserId";

            if (Task != null && Task.AssignedUserId != null)
                AssignUserComboBox.SelectedValue = Task.AssignedUserId;
            else
                AssignUserComboBox.SelectedIndex = 0;
        }




        private void LoadComboBoxes()
        {
            PriorityComboBox.ItemsSource = _context.Priorities.ToList();
            PriorityComboBox.DisplayMemberPath = "Name";
            PriorityComboBox.SelectedValuePath = "PriorityId";

            StatusComboBox.ItemsSource = _context.Statuses.ToList();
            StatusComboBox.DisplayMemberPath = "Name";
            StatusComboBox.SelectedValuePath = "StatusId";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Tiêu đề không được để trống.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (PriorityComboBox.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn độ ưu tiên.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (StatusComboBox.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn trạng thái.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (DueDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Vui lòng chọn ngày hết hạn.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Task.Title = TitleTextBox.Text.Trim();
            Task.Description = DescriptionTextBox.Text?.Trim();
            Task.PriorityId = (int)PriorityComboBox.SelectedValue;
            Task.StatusId = (int)StatusComboBox.SelectedValue;
            Task.StartAt = DateTime.Now;
            Task.DueAt = DueDatePicker.SelectedDate;
            Task.AssignedUserId = (int?)AssignUserComboBox.SelectedValue;

            DialogResult = true;
        }

    }
}


