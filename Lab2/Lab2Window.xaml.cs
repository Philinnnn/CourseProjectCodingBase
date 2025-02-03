using System.Windows;

namespace CourseProject.Lab2
{
    public partial class Lab2Window : Window
    {
        private IEncodingAlgorithm algorithm;

        public Lab2Window()
        {
            InitializeComponent();
        }

        private void RunHuffman(object sender, RoutedEventArgs e)
        {
            algorithm = new HuffmanCoding();
            RunAlgorithm();
        }

        private void RunShannonFano(object sender, RoutedEventArgs e)
        {
            algorithm = new ShannonFanoCoding();
            RunAlgorithm();
        }

        private void RunReedSolomon(object sender, RoutedEventArgs e)
        {
            algorithm = new ReedSolomonCoding();
            RunAlgorithm();
        }

        private void RunAlgorithm()
        {
            string input = InputTextBox.Text;

            string encodedText = algorithm.Encode(input);

            string decodedText = algorithm.Decode(encodedText);

            ResultText.Text = $"Закодированное сообщение: {encodedText}\n";
            ResultText.Text += $"Декодированное сообщение: {decodedText}\n";

            bool isCorrect = input == decodedText;
            ResultText.Text += $"Корректность декодирования: {(isCorrect ? "Успешно" : "Ошибка")}\n";

            double efficiency = algorithm.CalculateEfficiency(input, encodedText);
            ResultText.Text += $"Эффективность кодирования: {efficiency:P2}";
        }
    }
}
