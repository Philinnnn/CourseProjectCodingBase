using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CourseProject
{
    class LabSelector
    {
        public void OpenLab(int labNumber)
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
