using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using BitMiracle.LibJpeg;

namespace CourseProject.Lab5
{
    public partial class Lab5Window : Window
    {
        private BitmapImage originalBitmap;
        private BitmapImage compressedBitmap;
        private byte[] compressedJpegBytes;

        public Lab5Window()
        {
            InitializeComponent();
        }

        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.bmp;*.png)|*.bmp;*.png";
            if (openFileDialog.ShowDialog() == true)
            {
                originalBitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                OriginalImage.Source = originalBitmap;
            }
        }

        private void CompressButton_Click(object sender, RoutedEventArgs e)
        {
            if (originalBitmap == null)
            {
                MessageBox.Show("Пожалуйста, загрузите изображение.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                int sliderValue = (int)CompressionSlider.Value;
                
                int quality = 100 - sliderValue;
                
                compressedJpegBytes = CompressJpeg(originalBitmap, quality);
                
                MessageBox.Show($"Размер сжатого JPEG (степень сжатия {sliderValue}, качество {quality}): {compressedJpegBytes.Length / 1024.0 / 1024.0:F2} MB", "Информация");
                
                compressedBitmap = ByteArrayToBitmapImage(compressedJpegBytes);
                CompressedImage.Source = compressedBitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сжатии изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (compressedBitmap == null || compressedJpegBytes == null)
            {
                MessageBox.Show("Пожалуйста, выполните сжатие изображения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JPEG files (*.jpg)|*.jpg|PNG files (*.png)|*.png|BMP files (*.bmp)|*.bmp";
            if (saveFileDialog.ShowDialog() == true)
            {
                string extension = Path.GetExtension(saveFileDialog.FileName).ToLower();
                using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    if (extension == ".jpg")
                    {
                        fileStream.Write(compressedJpegBytes, 0, compressedJpegBytes.Length);
                    }
                    else
                    {
                        BitmapEncoder encoder;
                        if (extension == ".png")
                        {
                            encoder = new PngBitmapEncoder();
                        }
                        else
                        {
                            encoder = new BmpBitmapEncoder();
                        }
                        encoder.Frames.Add(BitmapFrame.Create(compressedBitmap));
                        encoder.Save(fileStream);
                    }
                }
            }
        }
        
        private byte[] CompressJpeg(BitmapImage bitmapImage, int quality)
        {
            FormatConvertedBitmap convertedBitmap = new FormatConvertedBitmap();
            convertedBitmap.BeginInit();
            convertedBitmap.Source = bitmapImage;
            convertedBitmap.DestinationFormat = PixelFormats.Bgra32;
            convertedBitmap.EndInit();

            int width = convertedBitmap.PixelWidth;
            int height = convertedBitmap.PixelHeight;
            int stride = width * 4;
            byte[] pixelData = new byte[height * stride];
            
            convertedBitmap.CopyPixels(pixelData, stride, 0);
            
            SampleRow[] rows = new SampleRow[height];
            for (int y = 0; y < height; y++)
            {
                byte[] rowData = new byte[width * 3];
                for (int x = 0; x < width; x++)
                {
                    int srcIndex = y * stride + x * 4;
                    int dstIndex = x * 3;
                    rowData[dstIndex] = pixelData[srcIndex + 2];     // R
                    rowData[dstIndex + 1] = pixelData[srcIndex + 1]; // G
                    rowData[dstIndex + 2] = pixelData[srcIndex];     // B
                    // Игнорирование Alpha-канала (pixelData[srcIndex + 3])
                }
                rows[y] = new SampleRow(rowData, width, (byte)8, (byte)3);
                // row, sampleCount, bitsPerComponent, componentsPerSample
            }
            
            JpegImage jpegImage = new JpegImage(rows, Colorspace.RGB);
            
            CompressionParameters parameters = new CompressionParameters
            {
                Quality = quality,
                SimpleProgressive = false
            };
            
            using (MemoryStream outputStream = new MemoryStream())
            {
                jpegImage.WriteJpeg(outputStream, parameters);
                return outputStream.ToArray();
            }
        }
        
        private BitmapImage ByteArrayToBitmapImage(byte[] byteArray)
        {
            using (MemoryStream memory = new MemoryStream(byteArray))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }
    }
}