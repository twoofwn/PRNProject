using PRNProject.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // Sử dụng ObservableCollection
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;

namespace PRNProject.Windows
{
    public partial class AddEditProjectWindow : Window
    {
        public Project Project { get; private set; }
        private readonly MyTaskContext _context = new MyTaskContext();

        private ObservableCollection<User> _selectedMembers;
        private User _projectOwner;
        public List<string> PredefinedColors { get; set; }

        public AddEditProjectWindow(Project project = null)
        {
            InitializeComponent();

            PredefinedColors = new List<string>
    {
        "#E63946", "#F1FAEE", "#A8DADC", "#457B9D", "#1D3557",
        "#FF6B6B", "#FFD166", "#06D6A0", "#118AB2", "#073B4C",
        "#F4A261", "#E76F51", "#8ECAE6", "#219EBC", "#2A9D8F",
        "#9B5DE5", "#F15BB5", "#FEE440", "#00F5D4", "#00BBF9"
    };
            ColorPickerListBox.ItemsSource = PredefinedColors;

            Project = project ?? new Project();
            _selectedMembers = new ObservableCollection<User>();

            if (project != null)
            {
                Project = _context.Projects
                    .Include(p => p.OwnerUser)
                    .Include(p => p.ProjectMembers)
                        .ThenInclude(pm => pm.User)
                    .FirstOrDefault(p => p.ProjectId == project.ProjectId);

                TitleTextBox.Text = Project.Title;
                DescriptionTextBox.Text = Project.Description;
                _projectOwner = Project.OwnerUser;

                if (!string.IsNullOrEmpty(Project.ColorHex) && PredefinedColors.Contains(Project.ColorHex))
                {
                    ColorPickerListBox.SelectedItem = Project.ColorHex;
                }
                else
                {
                    ColorPickerListBox.SelectedIndex = 0;
                }

                foreach (var member in Project.ProjectMembers)
                {
                    _selectedMembers.Add(member.User);
                }
            }
            else
            {
                ColorPickerListBox.SelectedIndex = 0;
            }

            SelectedMembersListBox.ItemsSource = _selectedMembers;
        }

        private void MemberSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = MemberSearchTextBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchText))
            {
                SearchPopup.IsOpen = false;
                return;
            }

            var excludedUserIds = _selectedMembers.Select(u => u.UserId).ToList();
            if (_projectOwner != null)
            {
                excludedUserIds.Add(_projectOwner.UserId);
            }

            var results = _context.Users
                .Where(u => u.Username.ToLower().Contains(searchText) && !excludedUserIds.Contains(u.UserId))
                .Take(10)
                .ToList();

            if (results.Any())
            {
                SearchResultsListBox.ItemsSource = results;
                SearchPopup.IsOpen = true;
            }
            else
            {
                SearchPopup.IsOpen = false;
            }
        }

        private void SearchResultsListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (SearchResultsListBox.SelectedItem is User selectedUser)
            {
                MemberSearchTextBox.Text = selectedUser.Username;
                SearchPopup.IsOpen = false;
            }
        }
        private void AddMemberButton_Click(object sender, RoutedEventArgs e)
        {
            var userToAdd = SearchResultsListBox.SelectedItem as User;

            if (userToAdd == null)
            {
                var username = MemberSearchTextBox.Text.Trim();
                userToAdd = _context.Users.FirstOrDefault(u => u.Username == username);
            }

            if (userToAdd != null && !_selectedMembers.Any(u => u.UserId == userToAdd.UserId))
            {
                _selectedMembers.Add(userToAdd);
                MemberSearchTextBox.Clear();
                SearchPopup.IsOpen = false;
            }
            else
            {
                MessageBox.Show("Không tìm thấy người dùng hoặc người dùng đã được thêm.", "Thông báo");
            }
        }

        private void RemoveMemberButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is User userToRemove)
            {
                _selectedMembers.Remove(userToRemove);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Tiêu đề không được để trống.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Project.Title = TitleTextBox.Text;
            Project.Description = DescriptionTextBox.Text;

            Project.ColorHex = ColorPickerListBox.SelectedItem as string;

            var currentMemberIds = Project.ProjectMembers.Select(pm => pm.UserId).ToList();
            var selectedMemberIds = _selectedMembers.Select(u => u.UserId).ToList();

            var membersToRemove = Project.ProjectMembers.Where(pm => !selectedMemberIds.Contains(pm.UserId)).ToList();
            foreach (var member in membersToRemove)
            {
                _context.ProjectMembers.Remove(member);
            }

            var memberIdsToAdd = selectedMemberIds.Except(currentMemberIds).ToList();
            foreach (var userId in memberIdsToAdd)
            {
                Project.ProjectMembers.Add(new ProjectMember { UserId = userId, ProjectId = Project.ProjectId });
            }

            _context.Entry(Project).State = EntityState.Modified;
            _context.SaveChanges();

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}