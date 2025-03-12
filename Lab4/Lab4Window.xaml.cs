using System.Windows;
using Microsoft.Win32;
using System.IO;
using CourseProject.Lab2;
using System.Runtime.Serialization.Json;
using System.Text;

namespace CourseProject.Lab4
{
    public partial class Lab4Window : Window
    {
        private string inputText = "";
        private string encodedText = "";
        private string? saveDirectory = "C:\\Users\\Arman\\Desktop"; 
        
        private HuffmanCoding huffmanCoder = new();
        private Dictionary<char, string>? huffmanTable;
        public Lab4Window()
        {
            InitializeComponent();
        }
        
        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (saveDirectory != null)
            {
                var huffmanTablePath = Path.Combine(saveDirectory, "HuffmanTable.json");
                openFileDialog.Filter = "Text files (*.txt)|*.txt|Binary files (*.bin)|*.bin";
                if (openFileDialog.ShowDialog() == true)
                {
                    var extension = Path.GetExtension(openFileDialog.FileName).ToLower();
                    if (extension == ".bin")
                    {
                        var fileBytes = File.ReadAllBytes(openFileDialog.FileName);
                        var binaryContent = Encoding.UTF8.GetString(fileBytes);
                        if (File.Exists(huffmanTablePath))
                        {
                            var serializer = new DataContractJsonSerializer(typeof(Dictionary<char, string>)); 
                            using (var stream = new FileStream(huffmanTablePath, FileMode.Open))
                            {
                                huffmanTable = serializer.ReadObject(stream) as Dictionary<char, string>;
                            }
                        }
                        huffmanCoder.HuffmanTable = huffmanTable;
                        var decodedText = huffmanCoder.Decode(binaryContent);
                        inputText = decodedText;
                        encodedText = binaryContent;
                    }
                    else
                    {
                        inputText = File.ReadAllText(openFileDialog.FileName);
                        encodedText = huffmanCoder.Encode(inputText);
                        huffmanTable = huffmanCoder.HuffmanTable;
                        ExportHuffmanTableToJson(huffmanTablePath);
                    }
                    InputTextBox.Text = inputText;
                    InputFileSizeTextBox.Text = (CalculateFileSize(inputText, false) / 1024).ToString("F2") + " KB";

                    CompressedTextBox.Text = encodedText;
                    CompressedFileSizeTextBox.Text = (CalculateFileSize(encodedText, true) / 1024).ToString("F2") + " KB";
                }
            }
        }
        
        private void SelectSaveDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = saveDirectory;
            dialog.Title = "Выберите директорию для сохранения";

            dialog.ValidateNames = false;
            dialog.CheckFileExists = false;
            dialog.CheckPathExists = true;

            dialog.FileName = "Folder Selection.";
            if (dialog.ShowDialog() == true)
            {
                saveDirectory = Path.GetDirectoryName(dialog.FileName);
            }
        }
        
        private void SaveTextFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(saveDirectory))
            {
                MessageBox.Show("Пожалуйста, выберите директорию для сохранения файла.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            var baseFileName = "DecodedFile.txt";
            
            File.WriteAllText(Path.Combine(saveDirectory, baseFileName), inputText, Encoding.UTF8);
            MessageBox.Show("Текстовый файл успешно сохранен по пути: " + Path.Combine(saveDirectory, baseFileName),
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void SaveBinaryFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(saveDirectory))
            {
                MessageBox.Show("Пожалуйста, выберите директорию для сохранения файла.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            var baseFileName = "CompressedFile.bin";
            
            File.WriteAllBytes(Path.Combine(saveDirectory, baseFileName), Encoding.UTF8.GetBytes(encodedText));
            MessageBox.Show("Бинарный файл успешно сохранен по пути: " + Path.Combine(saveDirectory, baseFileName),
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        public double CalculateFileSize(string file, bool isCompressed = false)
        {
            return isCompressed ? file.Length : file.Length * 8;
        }
        
        public void ExportHuffmanTableToJson(string filePath)
        {
            if (huffmanTable == null)
            {
                MessageBox.Show("Таблица Хаффмана пуста. Сначала выполните кодирование.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var serializer = new DataContractJsonSerializer(typeof(Dictionary<char, string>));
            using (var stream = new FileStream(filePath, FileMode.Create))
            using (var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, true, true, "  "))
            {
                serializer.WriteObject(writer, huffmanTable);
                writer.WriteWhitespace("\n");
            }
        }
    }
}
