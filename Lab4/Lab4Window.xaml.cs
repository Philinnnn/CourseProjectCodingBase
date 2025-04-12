using System.Windows;
using Microsoft.Win32;
using System.IO;
using CourseProject.Lab2;
using System.Runtime.Serialization.Json;
using System.Text;

namespace CourseProject.Lab4
{
    public partial class Lab4Window
    {
        private string inputText = "";
        private string encodedText = "";

        private string? saveDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Lab4");
        
        private HuffmanCoding huffmanCoder = new();
        
        public Lab4Window()
        {
            InitializeComponent();
        }
        
        private void ClearData()
        {
            inputText = "";
            encodedText = "";
            huffmanCoder.HuffmanTable = null;
            InputTextBox.Text = "Содержимое файла";
            CompressedTextBox.Text = "Сжатое содержимое";
            InputFileSizeTextBox.Text = "Размер исходного файла";
            CompressedFileSizeTextBox.Text = "Размер сжатого файла";
        }
        
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearData();
        }
        
        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            ClearData();
            
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|Binary files (*.bin)|*.bin";
            if (openFileDialog.ShowDialog() == true)
            {
                var fileDirectory = Path.GetDirectoryName(openFileDialog.FileName);
                if (fileDirectory != null)
                {
                    var huffmanTablePath = Path.Combine(fileDirectory, "HuffmanTable.json");
                    var extension = Path.GetExtension(openFileDialog.FileName).ToLower();
                    if (extension == ".bin")
                    {
                        var fileBytes = File.ReadAllBytes(openFileDialog.FileName);
                        var binaryContent = BitPacker.UnpackBits(fileBytes);
                        if (File.Exists(huffmanTablePath))
                        {
                            var serializer = new DataContractJsonSerializer(typeof(Dictionary<char, string>)); 
                            using (var stream = new FileStream(huffmanTablePath, FileMode.Open))
                            {
                                huffmanCoder.HuffmanTable = serializer.ReadObject(stream) as Dictionary<char, string>;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Файл таблицы HuffmanTable.json не найден в " + fileDirectory, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        if (huffmanCoder.HuffmanTable == null)
                        {
                            MessageBox.Show("Ошибка при загрузке таблицы Хаффмана.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        var decodedText = huffmanCoder.Decode(binaryContent);
                        inputText = decodedText;
                        encodedText = binaryContent;
                        CompressedFileSizeTextBox.Text = (CalculateFileSize(openFileDialog.FileName) / 1024).ToString("F2") + " KB";
                    }
                    else
                    {
                        inputText = File.ReadAllText(openFileDialog.FileName);
                        encodedText = huffmanCoder.Encode(inputText);
                        
                        if (huffmanCoder.HuffmanTable == null)
                        {
                            MessageBox.Show("Ошибка при создании таблицы Хаффмана.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var huffmanTablePathText = Path.Combine(Path.GetDirectoryName(openFileDialog.FileName) ?? string.Empty, "HuffmanTable.json");
                        ExportHuffmanTableToJson(huffmanTablePathText);
                        InputFileSizeTextBox.Text = (CalculateFileSize(openFileDialog.FileName) / 1024).ToString("F2") + " KB";
                    }
                }

                InputTextBox.Text = inputText ?? "Ошибка загрузки текста.";
                CompressedTextBox.Text = encodedText ?? "Ошибка кодирования текста.";
            }
        }
        
        private void SelectSaveDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = saveDirectory;
            dialog.Title = "Выберите директорию для сохранения";
            dialog.ValidateNames = false;
            dialog.CheckFileExists = false;
            dialog.CheckPathExists = true;
            dialog.FileName = "Выберите папку";
            if (dialog.ShowDialog() == true)
            {
                saveDirectory = Path.GetDirectoryName(dialog.FileName);
            }
        }
        
        private void SaveTextFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(saveDirectory))
            {
                MessageBox.Show("Пожалуйста, выберите директорию для сохранения файла.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(inputText))
            {
                MessageBox.Show("Нет данных для сохранения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            var baseFileName = "DecodedFile.txt";
            var filePath = Path.Combine(saveDirectory, baseFileName);
            File.WriteAllText(filePath, inputText, Encoding.UTF8);
            MessageBox.Show("Текстовый файл успешно сохранен по пути: " + filePath, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            InputFileSizeTextBox.Text = (CalculateFileSize(filePath) / 1024).ToString("F2") + " KB";
        }
        
        private void SaveBinaryFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(saveDirectory))
            {
                MessageBox.Show("Пожалуйста, выберите директорию для сохранения файла.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(encodedText))
            {
                MessageBox.Show("Нет данных для сохранения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            var baseFileName = "CompressedFile.bin";
            var filePath = Path.Combine(saveDirectory, baseFileName);
            File.WriteAllBytes(filePath, BitPacker.PackBits(encodedText));
            MessageBox.Show("Бинарный файл успешно сохранен по пути: " + filePath, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            CompressedFileSizeTextBox.Text = (CalculateFileSize(filePath) / 1024).ToString("F2") + " KB";
        }
        
        public double CalculateFileSize(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Length;
        }
        
        public void ExportHuffmanTableToJson(string filePath)
        {
            if (huffmanCoder.HuffmanTable == null)
            {
                MessageBox.Show("Таблица Хаффмана пуста. Сначала выполните кодирование.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var serializer = new DataContractJsonSerializer(typeof(Dictionary<char, string>));
            using (var stream = new FileStream(filePath, FileMode.Create))
            using (var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, true, true, "  "))
            {
                serializer.WriteObject(writer, huffmanCoder.HuffmanTable);
                writer.WriteWhitespace("\n");
            }
        }
    }
}

