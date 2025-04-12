using System.Windows;
using System.Windows.Controls;

namespace CourseProject
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenLab(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int labNumber))
            {
                LabSelector.OpenLab(labNumber);
            }
            Close();
        }
    }
}
