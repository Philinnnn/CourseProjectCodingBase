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

        private void RunAlgorithm(bool introduceError = false)
        {
            string input = InputTextBox.Text;
            string originalEncodedText = algorithm.Encode(input);

            string encodedTextForDecoding = originalEncodedText;

            string errorMessage = "";
            if (introduceError && algorithm is ReedSolomonCoding rs)
            {
                string corruptedText = rs.IntroduceError(originalEncodedText);
                errorMessage = $"Имитированное сообщение с ошибкой: {corruptedText}\n";
                encodedTextForDecoding = corruptedText;
            }

            string decodedText = algorithm.Decode(encodedTextForDecoding);

            ResultText.Text = $"Оригинальное закодированное сообщение: {originalEncodedText}\n";
            if (!string.IsNullOrEmpty(errorMessage))
                ResultText.Text += errorMessage;
            ResultText.Text += $"Декодированное сообщение: {decodedText}\n";

            bool isCorrect = input == decodedText;
            ResultText.Text += $"Корректность декодирования: {(isCorrect ? "Успешно" : "Ошибка")}\n";

            double efficiency = algorithm.CalculateEfficiency(input, originalEncodedText);
            ResultText.Text += $"Эффективность кодирования: {efficiency:P2}";
        }

        private void RunHuffman(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
                return;

            algorithm = new HuffmanCoding();
            RunAlgorithm();
        }

        private void RunShannonFano(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
                return;

            algorithm = new ShannonFanoCoding();
            RunAlgorithm();
        }
        private void RunReedSolomon(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
                return;

            algorithm = new ReedSolomonCoding();
            RunAlgorithm(true);
        }

        private void ClearFields(object sender, RoutedEventArgs e)
        {
            InputTextBox.Text = string.Empty;
            ResultText.Text = string.Empty;
        }
    }
}