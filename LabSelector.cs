using System.Windows;

namespace CourseProject
{
    public class LabSelector
    {
        public static void OpenLab(int labNumber)
        {
            Window? labWindow = labNumber switch
            {
                2 => new Lab2.Lab2Window(),
                _ => null
            };

            labWindow?.Show();
        }
    }
}
