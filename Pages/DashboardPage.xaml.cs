using Microsoft.EntityFrameworkCore;
using PRNProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;

namespace PRNProject.Pages
{
    public partial class DashboardPage : Page
    {
        private readonly MyTaskContext _context = new MyTaskContext();
        private User _currentUser;

        public SeriesCollection StatusSeries { get; set; }
        public SeriesCollection PrioritySeries { get; set; }
        public string[] PriorityLabels { get; set; }

        private class ProjectFilterItem
        {
            public int ProjectId { get; set; } 
            public string Title { get; set; }
        }

        public DashboardPage(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;

            StatusSeries = new SeriesCollection();
            PrioritySeries = new SeriesCollection();
            PriorityLabels = new string[0];

            DataContext = this;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Không tìm thấy người dùng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            LoadProjectFilter();
        }

        private void LoadProjectFilter()
        {
            var projects = _context.Projects
                .Where(p => p.OwnerUserId == _currentUser.UserId)
                .Select(p => new ProjectFilterItem { ProjectId = p.ProjectId, Title = p.Title })
                .ToList();

            // Thêm tùy chọn "Tất cả"
            projects.Insert(0, new ProjectFilterItem { ProjectId = 0, Title = "Tất cả Project (của tôi)" });

            ProjectFilterComboBox.ItemsSource = projects;
            ProjectFilterComboBox.DisplayMemberPath = "Title";
            ProjectFilterComboBox.SelectedValuePath = "ProjectId";
            ProjectFilterComboBox.SelectedIndex = 0;
        }

        private void ProjectFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) // Chỉ load khi trang đã sẵn sàng
            {
                LoadChartData();
            }
        }

        private void LoadChartData()
        {
            if (_currentUser == null || ProjectFilterComboBox.SelectedItem == null) return;

            var selectedFilter = (ProjectFilterItem)ProjectFilterComboBox.SelectedItem;
            int selectedProjectId = selectedFilter.ProjectId;

            IQueryable<Models.Task> tasksQuery;

            // Lọc
            if (selectedProjectId == 0) 
            {
                var userOwnedProjectIds = _context.Projects
                    .Where(p => p.OwnerUserId == _currentUser.UserId)
                    .Select(p => p.ProjectId);

                tasksQuery = _context.Tasks
                    .Where(t => t.ProjectId.HasValue && userOwnedProjectIds.Contains(t.ProjectId.Value));
            }
            else 
            {
                // nếu chọn 1 project cụ thể
                tasksQuery = _context.Tasks
                    .Where(t => t.ProjectId == selectedProjectId);
            }

            var tasks = tasksQuery
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .AsNoTracking()
                .ToList();

            // nhóm theo status, list có dạng : {sttId, sttName, taskCount}
            var statusSummary = tasks
                .GroupBy(t => new { t.Status.StatusId, t.Status.Name })
                .Select(g => new
                {
                    StatusId = g.Key.StatusId,
                    StatusName = g.Key.Name,
                    TaskCount = g.Count()       // Đếm số task của mỗi nhóm stt
                })
                .OrderBy(s => s.StatusId)
                .ToList();

            // Cập nhật vào PieChart
            StatusSeries.Clear();
            foreach (var item in statusSummary)
            {
                StatusSeries.Add(new PieSeries
                {
                    Title = item.StatusName,
                    Values = new ChartValues<int>(new[] { item.TaskCount }),
                    DataLabels = true,
                    LabelPoint = chartPoint => $"{chartPoint.Y} ({chartPoint.Participation:P0})"
                });
            }

            // nhóm theo status
            var prioritySummary = tasks
                .GroupBy(t => new { t.Priority.PriorityId, t.Priority.Name })
                .Select(g => new
                {
                    PriorityId = g.Key.PriorityId,
                    PriorityName = g.Key.Name,
                    TaskCount = g.Count()
                })
                .OrderBy(p => p.PriorityId)
                .ToList();

            // Vẽ biểu đồ cột
            PrioritySeries.Clear();
            PrioritySeries.Add(new ColumnSeries
            {
                Title = "Số lượng Task",
                Values = new ChartValues<int>(prioritySummary.Select(p => p.TaskCount)),
                DataLabels = true
            });

            PriorityLabels = prioritySummary.Select(p => p.PriorityName).ToArray();
            PriorityChart.AxisX[0].Labels = PriorityLabels;
        }

        private void Chart_DataClick(object sender, ChartPoint chartPoint)
        {
            var pieSeries = chartPoint.SeriesView as PieSeries;
            if (pieSeries != null)
            {
                MessageBox.Show($"Bạn đã chọn: {pieSeries.Title} (có {chartPoint.Y} task).");
            }
        }
    }
}