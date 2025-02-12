using Lab3;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CourseProject.Lab3
{
    public partial class Lab3Window : Window
    {
        private SignalProcessor processor;

        public Lab3Window()
        {
            InitializeComponent();
        }

        private void GenerateSignal(object sender, RoutedEventArgs e)
        {
            double frequency;
            double lossPercentage;
            if (!double.TryParse(InputTextBox.Text, out frequency))
            {
                frequency = 20;
            }
            if (!double.TryParse(LossPercentageTextBox.Text, out lossPercentage) || lossPercentage <= 0 || lossPercentage >= 100)
            {
                lossPercentage = 10;
            }
            // sampleRate = 1000 Гц, duration = 1 сек, noiseLevel = 0.2, lossPercentage = 0.1 (10%)
            processor = new SignalProcessor(1000, 1.0, frequency, 0.2, lossPercentage/100);
            MessageBox.Show($"Сигнал сгенерирован с частотой {frequency:F2} Гц и " +
                $"процентом потерь {lossPercentage:F2}.", "Генерация сигнала");
        }

        private void ShowGraphs(object sender, RoutedEventArgs e)
        {
            if (processor == null)
            {
                MessageBox.Show("Сначала сгенерируйте сигнал.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DrawGraph(OriginalCanvas, processor.OriginalSignal);
            DrawGraph(ProcessedCanvas, processor.ProcessedSignal);
            DrawGraph(FilteredCanvas, processor.FilteredSignal);
        }

        private void ClearGraphs(object sender, RoutedEventArgs e)
        {
            OriginalCanvas.Children.Clear();
            ProcessedCanvas.Children.Clear();
            FilteredCanvas.Children.Clear();
        }

        private void DrawGraph(Canvas canvas, double[] data)
        {
            if (data == null || data.Length == 0)
                return;

            canvas.Children.Clear();

            double width = canvas.Width;
            double height = canvas.Height;

            // Определяем минимальное и максимальное значение (исключая NaN)
            var validData = data.Where(x => !double.IsNaN(x));
            double min = validData.Min();
            double max = validData.Max();
            if (Math.Abs(max - min) < 1e-6)
                max = min + 1; // предотвращаем деление на 0

            Polyline polyline = new Polyline
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 1
            };

            for (int i = 0; i < data.Length; i++)
            {
                // Пропускаем точку
                if (double.IsNaN(data[i]))
                    continue;

                double x = i * (width / (data.Length - 1));
                // Масштабируем y
                double y = height - ((data[i] - min) / (max - min)) * height;
                polyline.Points.Add(new Point(x, y));
            }

            canvas.Children.Add(polyline);
        }
    }
}
