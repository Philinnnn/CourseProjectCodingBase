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
            if (LabList.SelectedItem is ListBoxItem selectedItem)
            {
                if (int.TryParse(selectedItem.Tag?.ToString(), out int labNumber))
                {
                    LabSelector.OpenLab(labNumber);
                }
                else
                {
                    MessageBox.Show("Неверный идентификатор лабораторной работы.");
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите лабораторную работу.");
            }
        }
    }
}
