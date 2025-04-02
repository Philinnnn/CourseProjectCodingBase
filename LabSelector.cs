using System.Windows;

namespace CourseProject
{
    public class LabSelector
    {
        public static void OpenLab(int labNumber)
        {
            Window? labWindow = labNumber switch
            {
                1 => new Lab1.Lab1Window(),
                2 => new Lab2.Lab2Window(),
                3 => new Lab3.Lab3Window(),
                4 => new Lab4.Lab4Window(),
                5 => new Lab5.Lab5Window(),
                _ => null
            };

            labWindow?.Show();
        }
    }
}
