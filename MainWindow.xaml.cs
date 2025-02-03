using System.Windows;

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
            if (LabList.SelectedIndex == 0)
            {
                var lab2Window = new Lab2.Lab2Window();
                lab2Window.Show();
            }
        }
    }
}
