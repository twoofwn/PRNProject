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
    /// Interaction logic for AddEditProjectWindow.xaml
    /// </summary>
    public partial class AddEditProjectWindow : Window
    {
        public Project Project { get; private set; }

        public AddEditProjectWindow(Project project = null)
        {
            InitializeComponent();

            Project = project ?? new Project();
            if (project != null)
            {
                TitleTextBox.Text = Project.Title;
                DescriptionTextBox.Text = Project.Description;
                ColorHexTextBox.Text = Project.ColorHex;
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
            Project.ColorHex = ColorHexTextBox.Text;

            this.DialogResult = true; // Báo hiệu lưu thành công
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
