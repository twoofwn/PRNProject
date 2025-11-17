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

namespace PRNProject.Pages 
{
    /// <summary>
    /// Interaction logic for TaskDetailWindow.xaml
    /// </summary>
    public partial class TaskDetailWindow : Window 
    {
        private readonly MyTaskContext _context = new MyTaskContext();
        private Models.Task _currentTask;
        private readonly Models.User _currentUser;

        public TaskDetailWindow(Models.Task task, Models.User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;

            _currentTask = _context.Tasks
                                   .Include(t => t.Tags) 
                                   .Single(t => t.TaskId == task.TaskId);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTaskData();
            LoadComboBoxes();
            LoadComments();
            LoadAvailableTags();
        }

        private void LoadTaskData()
        {
            if (_currentTask == null) return;

            TitleTextBox.Text = _currentTask.Title;
            DescriptionTextBox.Text = _currentTask.Description;
            DueDatePicker.SelectedDate = _currentTask.DueAt;

            LoadAssignedTags();
        }

        private void LoadComboBoxes()
        {
            // Load Projects (chỉ của user hiện tại)
            ProjectComboBox.ItemsSource = _context.Projects.Where(p => p.OwnerUserId == _currentUser.UserId).ToList();
            ProjectComboBox.SelectedValue = _currentTask.ProjectId;

            // Load Statuses
            StatusComboBox.ItemsSource = _context.Statuses.ToList();
            StatusComboBox.SelectedValue = _currentTask.StatusId;

            // Load Priorities
            PriorityComboBox.ItemsSource = _context.Priorities.ToList();
            PriorityComboBox.SelectedValue = _currentTask.PriorityId;

        }


        private void LoadComments()
        {
            var comments = _context.Comments
                .Include(c => c.AuthorUser) 
                .Where(c => c.TaskId == _currentTask.TaskId)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
            CommentsListView.ItemsSource = comments;
        }

        private void LoadAssignedTags()
        {
            var assignedTags = _currentTask.Tags.ToList();
            TagsItemsControl.ItemsSource = assignedTags;
        }

        private void LoadAvailableTags()
        {
            var allUserTags = _context.Tags
                                    .Where(t => t.OwnerUserId == _currentUser.UserId)
                                    .ToList();

            var assignedTagIds = _currentTask.Tags.Select(t => t.TagId).ToHashSet();

            AvailableTagsComboBox.ItemsSource = allUserTags.Where(t => !assignedTagIds.Contains(t.TagId)).ToList();
            AvailableTagsComboBox.SelectedIndex = 0;
        }



        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                {
                    return;
                }
                _currentTask.Title = TitleTextBox.Text;
                _currentTask.Description = DescriptionTextBox.Text;
                _currentTask.ProjectId = (int?)ProjectComboBox.SelectedValue;
                _currentTask.StatusId = (int)StatusComboBox.SelectedValue;
                _currentTask.PriorityId = (int)PriorityComboBox.SelectedValue;
                _currentTask.DueAt = DueDatePicker.SelectedDate;

                _currentTask.UpdatedAt = DateTime.UtcNow;

                _context.SaveChanges();
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu task: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Tiêu đề không được để trống!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (DueDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Ngày hết hạn không được để trống!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (DueDatePicker.SelectedDate < DateTime.Today )
            {
                MessageBox.Show("Ngày hết hạn không được ở quá khứ!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true; 
        }


        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTask.StatusId == 3)
            {
                MessageBox.Show("Không thể xóa task có trạng thái đã hoàn thành");
                return;
            }
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa task '{_currentTask.Title}' không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Tasks.Remove(_currentTask);
                    _context.SaveChanges();
                    this.DialogResult = true; 
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa task: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; 
            this.Close();
        }

        private void PostCommentButton_Click(object sender, RoutedEventArgs e)
        {
            string commentBody = NewCommentTextBox.Text;
            if (string.IsNullOrWhiteSpace(commentBody))
            {
                MessageBox.Show("Vui lòng nhập nội dung bình luận.");
                return;
            }

            var newComment = new Models.Comment
            {
                TaskId = _currentTask.TaskId,
                AuthorUserId = _currentUser.UserId,
                Body = commentBody,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(newComment);
            _context.SaveChanges();

            NewCommentTextBox.Clear();
            LoadComments(); 
        }

        private void AddTagButton_Click(object sender, RoutedEventArgs e)
        {
            if (AvailableTagsComboBox.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn một tag để gán.");
                return;
            }

            int tagId = (int)AvailableTagsComboBox.SelectedValue;

            var tagToAdd = _context.Tags.Find(tagId);
            if (tagToAdd == null) return;

            bool alreadyExists = _currentTask.Tags.Any(t => t.TagId == tagId);
            if (!alreadyExists)
            {
                _currentTask.Tags.Add(tagToAdd);
                _context.SaveChanges();

                LoadAssignedTags();
                LoadAvailableTags();
            }
        }

        private void RemoveTagButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int tagId)
            {
                var tagToRemove = _currentTask.Tags
                                        .SingleOrDefault(t => t.TagId == tagId);

                if (tagToRemove != null)
                {
                    _currentTask.Tags.Remove(tagToRemove);
                    _context.SaveChanges();

                    LoadAssignedTags();
                    LoadAvailableTags();
                }
            }
        }
    }
}


