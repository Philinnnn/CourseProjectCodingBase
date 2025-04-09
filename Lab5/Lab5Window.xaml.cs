using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace CourseProject.Lab5
{
    public partial class Lab5Window : Window
    {
        private BitmapImage originalBitmap;
        private WriteableBitmap compressedBitmap;
        private WriteableBitmap restoredBitmap;

        public Lab5Window()
        {
            InitializeComponent();
            CompressionSlider.ValueChanged += CompressionSlider_ValueChanged;
        }

        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.bmp;*.jpg)|*.png;*.bmp;*.jpg|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    originalBitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                    OriginalImage.Source = originalBitmap;
                    CompressedImage.Source = null;

                    string extension = System.IO.Path.GetExtension(openFileDialog.FileName).ToLower();
                    if (extension == ".jpg" || extension == ".jpeg")
                    {
                        CompressButton.Visibility = Visibility.Collapsed;
                        RestoreButton.Visibility = Visibility.Visible;
                        SliderLabel.Visibility = Visibility.Collapsed;
                        CompressionSlider.Visibility = Visibility.Collapsed;
                        CompressionTextBox.Visibility = Visibility.Collapsed;
                    }
                    else // .png или .bmp
                    {
                        CompressButton.Visibility = Visibility.Visible;
                        RestoreButton.Visibility = Visibility.Collapsed;
                        SliderLabel.Visibility = Visibility.Visible;
                        CompressionSlider.Visibility = Visibility.Visible;
                        CompressionTextBox.Visibility = Visibility.Visible;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void CompressButton_Click(object sender, RoutedEventArgs e)
        {
            if (originalBitmap == null)
            {
                MessageBox.Show("Сначала загрузите изображение!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Подготовка данных в UI-потоке
                int width = originalBitmap.PixelWidth;
                int height = originalBitmap.PixelHeight;
                int stride = width * 4;
                byte[] pixels = new byte[height * stride];

                WriteableBitmap tempBitmap = new WriteableBitmap(originalBitmap);
                tempBitmap.CopyPixels(pixels, stride, 0);

                // Подготовка UI
                ProgressBar.Visibility = Visibility.Visible;
                CompressButton.IsEnabled = false;
                RestoreButton.IsEnabled = false;
                SaveImageButton.IsEnabled = false;

                // Асинхронное выполнение сжатия в фоновом потоке
                (byte[] compressedPixels, byte[] restoredPixels, int resultWidth, int resultHeight, int resultStride) = await Task.Run(() =>
                {
                    return JpegCompressor.Compress(pixels, width, height);
                });

                // Обновление UI в UI-потоке
                Dispatcher.Invoke(() =>
                {
                    compressedBitmap = new WriteableBitmap(resultWidth, resultHeight, 96, 96, PixelFormats.Bgra32, null);
                    compressedBitmap.WritePixels(new Int32Rect(0, 0, resultWidth, resultHeight), compressedPixels, resultStride, 0);

                    restoredBitmap = new WriteableBitmap(resultWidth, resultHeight, 96, 96, PixelFormats.Bgra32, null);
                    restoredBitmap.WritePixels(new Int32Rect(0, 0, resultWidth, resultHeight), restoredPixels, resultStride, 0);

                    CompressedImage.Source = restoredBitmap;
                    ProgressBar.Visibility = Visibility.Collapsed;
                    CompressButton.IsEnabled = true;
                    RestoreButton.IsEnabled = true;
                    SaveImageButton.IsEnabled = true;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Ошибка при сжатии: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    ProgressBar.Visibility = Visibility.Collapsed;
                    CompressButton.IsEnabled = true;
                    RestoreButton.IsEnabled = true;
                    SaveImageButton.IsEnabled = true;
                });
            }
        }

        private async void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (originalBitmap == null)
            {
                MessageBox.Show("Сначала загрузите изображение!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ProgressBar.Visibility = Visibility.Visible;
                CompressButton.IsEnabled = false;
                RestoreButton.IsEnabled = false;
                SaveImageButton.IsEnabled = false;

                if (CompressedImage.Source == null) // Если это JPG и еще не восстановлено
                {
                    int width = originalBitmap.PixelWidth;
                    int height = originalBitmap.PixelHeight;
                    int stride = width * 4;
                    byte[] pixels = new byte[height * stride];

                    WriteableBitmap tempBitmap = new WriteableBitmap(originalBitmap);
                    tempBitmap.CopyPixels(pixels, stride, 0);

                    (byte[] compressedPixels, byte[] restoredPixels, int resultWidth, int resultHeight, int resultStride) = await Task.Run(() =>
                    {
                        return JpegCompressor.Compress(pixels, width, height);
                    });

                    Dispatcher.Invoke(() =>
                    {
                        compressedBitmap = new WriteableBitmap(resultWidth, resultHeight, 96, 96, PixelFormats.Bgra32, null);
                        compressedBitmap.WritePixels(new Int32Rect(0, 0, resultWidth, resultHeight), compressedPixels, resultStride, 0);

                        restoredBitmap = new WriteableBitmap(resultWidth, resultHeight, 96, 96, PixelFormats.Bgra32, null);
                        restoredBitmap.WritePixels(new Int32Rect(0, 0, resultWidth, resultHeight), restoredPixels, resultStride, 0);

                        CompressedImage.Source = restoredBitmap;
                        ProgressBar.Visibility = Visibility.Collapsed;
                        CompressButton.IsEnabled = true;
                        RestoreButton.IsEnabled = true;
                        SaveImageButton.IsEnabled = true;
                    });
                }
                else // Сброс результата сжатия
                {
                    Dispatcher.Invoke(() =>
                    {
                        OriginalImage.Source = originalBitmap;
                        CompressedImage.Source = null;
                        CompressButton.Visibility = Visibility.Visible;
                        RestoreButton.Visibility = Visibility.Collapsed;
                        ProgressBar.Visibility = Visibility.Collapsed;
                        CompressButton.IsEnabled = true;
                        RestoreButton.IsEnabled = true;
                        SaveImageButton.IsEnabled = true;
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Ошибка при восстановлении: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    ProgressBar.Visibility = Visibility.Collapsed;
                    CompressButton.IsEnabled = true;
                    RestoreButton.IsEnabled = true;
                    SaveImageButton.IsEnabled = true;
                });
            }
        }

        private void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (CompressedImage.Source == null || compressedBitmap == null || restoredBitmap == null)
            {
                MessageBox.Show("Сначала выполните сжатие или восстановление изображения!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JPEG Image (*.jpg)|*.jpg|PNG Image (*.png)|*.png"
            };

            string extension = System.IO.Path.GetExtension(OriginalImage.Source.ToString()).ToLower();
            if (extension == ".jpg" || extension == ".jpeg")
            {
                saveFileDialog.FileName = "RestoredImg.png";
                saveFileDialog.DefaultExt = "png";
            }
            else
            {
                saveFileDialog.FileName = "CompressedImg.jpg";
                saveFileDialog.DefaultExt = "jpg";
            }

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    extension = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();
                    BitmapEncoder encoder;
                    BitmapSource source;

                    if (extension == ".jpg")
                    {
                        encoder = new JpegBitmapEncoder { QualityLevel = (int)CompressionSlider.Value };
                        source = compressedBitmap;
                    }
                    else // .png
                    {
                        encoder = new PngBitmapEncoder();
                        source = restoredBitmap;
                    }

                    encoder.Frames.Add(BitmapFrame.Create(source));
                    using (var stream = saveFileDialog.OpenFile())
                    {
                        encoder.Save(stream);
                    }
                    MessageBox.Show("Изображение успешно сохранено!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CompressionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CompressionTextBox != null)
            {
                CompressionTextBox.Text = CompressionSlider.Value.ToString("F0");
            }
        }
    }
}