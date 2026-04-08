using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CozyTest.Converters
{
    public class NullImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return null;

            string path = value.ToString();

            // Если путь относительный - преобразуем в абсолютный
            if (!Path.IsPathRooted(path))
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                path = Path.Combine(baseDir, path);
            }

            if (!File.Exists(path))
                return null;

            try
            {
                // Создаем BitmapImage с правильными настройками
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(path);
                bitmap.EndInit();
                bitmap.Freeze(); // Важно для производительности
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}