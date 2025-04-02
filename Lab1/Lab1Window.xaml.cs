using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CourseProject.Lab1
{
    public partial class Lab1Window : Window
    {
        public Lab1Window()
        {
            InitializeComponent();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && (textBox.Text == "Введите символы через пробел" || textBox.Text == "Введите вероятности через пробел"))
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                if (textBox.Name == "SymbolsTextBox")
                {
                    textBox.Text = "Введите символы через пробел";
                }
                else if (textBox.Name == "ProbabilitiesTextBox")
                {
                    textBox.Text = "Введите вероятности через пробел";
                }
                textBox.Foreground = Brushes.Gray;
            }
        }

        private void Task1Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var symbols = SymbolsTextBox.Text.Split(' ');
                var probabilities = ProbabilitiesTextBox.Text.Split(' ').Select(double.Parse).ToArray();
                if (symbols.Length != probabilities.Length)
                {
                    throw new Exception("Количество символов и\nвероятностей должно совпадать.");
                }

                double sum = 0.0d;
                ResultTextBox.Clear();

                for (int i = 0; i < symbols.Length; i++)
                {
                    sum += probabilities[i] * Math.Log2(probabilities[i]);
                    ResultTextBox.AppendText($"{i + 1} итерация = {sum}\n");
                }
                sum = -sum;
                ResultTextBox.AppendText($"Энтропия: {Math.Round(sum, 2)}\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Task2Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var symbols = SymbolsTextBox.Text.Split(' ');
                var probabilities = ProbabilitiesTextBox.Text.Split(' ').Select(double.Parse).ToArray();
                if (symbols.Length != probabilities.Length)
                {
                    throw new Exception("Количество символов и\nвероятностей должно совпадать.");
                }

                var symbolProbabilities = symbols.Zip(probabilities, (s, p) => new { Symbol = s, Probability = p })
                                                 .ToDictionary(x => x.Symbol, x => x.Probability);
                var nodes = symbolProbabilities
                    .Select(kvp => new Node { Symbol = kvp.Key, Probability = kvp.Value })
                    .ToList();
                ResultTextBox.Clear();

                while (nodes.Count > 1)
                {
                    nodes = nodes.OrderBy(node => node.Probability).ToList();
                    var left = nodes[0];
                    var right = nodes[1];

                    var newNode = new Node
                    {
                        Symbol = $"({left.Symbol}+{right.Symbol})",
                        Probability = left.Probability + right.Probability,
                        Left = left,
                        Right = right
                    };

                    nodes.Remove(left);
                    nodes.Remove(right);
                    nodes.Add(newNode);

                    ResultTextBox.AppendText($"Объединили {left.Symbol} ({left.Probability}) и {right.Symbol} ({right.Probability}) в {newNode.Symbol} ({newNode.Probability})\n");
                }
                var root = nodes[0];
                ResultTextBox.AppendText("\nКоды Хаффмана:\n");
                GenerateHuffmanCodes(root, "");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Task3Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var probabilities = ProbabilitiesTextBox.Text.Split(' ').Select(double.Parse).ToArray();
                string[] binaryCodes = { "00", "10", "11" };
                string[] huffmanCodes = { "0", "10", "11" };
                int alphabetPower = probabilities.Length;
                double entropy = CalculateEntropy(probabilities, alphabetPower);
                ResultTextBox.Clear();
                ResultTextBox.AppendText($"Энтропия: {entropy}\n");
                double averageCodeLengthBinary = CalculateAverageWordLength(binaryCodes, probabilities);
                double averageCodeLengthHuffman = CalculateAverageWordLength(huffmanCodes, probabilities);
                ResultTextBox.AppendText($"Средняя длина кодового слова при двоичном кодировании: {averageCodeLengthBinary}\n");
                ResultTextBox.AppendText($"Средняя длина кодового слова при кодировании Хаффмана: {averageCodeLengthHuffman}\n");
                ResultTextBox.AppendText($"Эффективность двоичного кодирования: {CalculateEfficiency(entropy, averageCodeLengthBinary)}\n");
                ResultTextBox.AppendText($"Эффективность кодирования Хаффмана: {CalculateEfficiency(entropy, averageCodeLengthHuffman)}\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static double CalculateEntropy(double[] probabilities, int alphabetPower)
        {
            double sum = 0;
            for (int i = 0; i < alphabetPower; i++)
            {
                sum += probabilities[i] * Math.Log2(probabilities[i]);
            }
            sum = -sum;
            return Math.Round(sum, 2);
        }

        private static double CalculateEfficiency(double entropy, double averageCodeWordLength)
        {
            return entropy / averageCodeWordLength;
        }

        private static double CalculateAverageWordLength(string[] codes, double[] probabilities)
        {
            double sum = 0;
            int i = 0;
            foreach (string code in codes)
            {
                sum += code.Length * probabilities[i];
                i++;
            }
            return sum;
        }

        private void GenerateHuffmanCodes(Node node, string code)
        {
            if (node.Left == null && node.Right == null)
            {
                ResultTextBox.AppendText($"Символ: {node.Symbol}, Код: {code}\n");
                return;
            }
            if (node.Left != null)
                GenerateHuffmanCodes(node.Left, code + "0");

            if (node.Right != null)
                GenerateHuffmanCodes(node.Right, code + "1");
        }

        public class Node
        {
            public string? Symbol { get; set; }
            public double Probability { get; set; }
            public Node? Left { get; set; }
            public Node? Right { get; set; }
        }
    }
}