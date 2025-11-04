using Microsoft.EntityFrameworkCore;
using PRNProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PRNProject.Pages
{
    public partial class CalendarPage : Page
    {
        private readonly User _currentUser;

        public CalendarPage(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (TaskCalendar.SelectedDate.HasValue)
            {
                LoadTasksForDateRange(TaskCalendar.SelectedDate.Value);
            }
            else
            {
                LoadAllUpcomingTasks();
            }
        }

        private void TaskCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TaskCalendar.SelectedDate.HasValue)
            {
                DateTime selectedDate = TaskCalendar.SelectedDate.Value;
                LoadTasksForDateRange(selectedDate);
            }
        }

        private void ShowAllButton_Click(object sender, RoutedEventArgs e)
        {
            TaskCalendar.SelectedDate = null;
            TaskCalendar.DisplayDate = DateTime.Today;
            LoadAllUpcomingTasks();
        }

        private void LoadAllUpcomingTasks()
        {
            if (_currentUser == null) return;

            SelectedDateTextBlock.Text = "Tất cả nhiệm vụ sắp tới";
            List<Models.Task> tasks;

            using (var context = new MyTaskContext())
            {
                tasks = context.Tasks
                    .AsNoTracking()
                    .Include(t => t.Project)
                    .Where(t => t.OwnerUserId == _currentUser.UserId &&
                                t.StatusId != 3 &&
                                t.DueAt.HasValue)
                    .OrderBy(t => t.DueAt)
                    .ToList();
            }

            TasksForDayListView.ItemsSource = tasks;
            UpdateNoTasksMessage(tasks, "Không có nhiệm vụ nào sắp tới.");
        }

        private void LoadTasksForDateRange(DateTime date)
        {
            if (_currentUser == null) return;

            SelectedDateTextBlock.Text = $"Nhiệm vụ trong ngày: {date:dd/MM/yyyy}";
            List<Models.Task> tasks;

            using (var context = new MyTaskContext())
            {
                tasks = context.Tasks
                    .AsNoTracking()
                    .Include(t => t.Project)
                    .Where(t => t.OwnerUserId == _currentUser.UserId &&
                                t.StatusId != 3 &&
                                t.StartAt.HasValue &&
                                t.DueAt.HasValue &&
                                t.StartAt.Value.Date <= date.Date &&
                                t.DueAt.Value.Date >= date.Date)
                    .OrderBy(t => t.DueAt)
                    .ToList();
            }

            TasksForDayListView.ItemsSource = tasks;
            UpdateNoTasksMessage(tasks, "Không có nhiệm vụ nào hoạt động trong ngày này.");
        }

        private void UpdateNoTasksMessage(List<Models.Task> tasks, string message)
        {
            NoTasksTextBlock.Text = message;
            if (tasks.Any())
            {
                NoTasksTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                NoTasksTextBlock.Visibility = Visibility.Visible;
            }
        }
    }
}