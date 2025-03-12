﻿using System.Windows;

namespace CourseProject
{
    public class LabSelector
    {
        public static void OpenLab(int labNumber)
        {
            Window? labWindow = labNumber switch
            {
                2 => new Lab2.Lab2Window(),
                3 => new Lab3.Lab3Window(),
                4 => new Lab4.Lab4Window(),
                _ => null
            };

            labWindow?.Show();
        }
    }
}
