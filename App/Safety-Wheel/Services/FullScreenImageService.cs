using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CozyTest.Services
{
    public static class FullScreenImageService
    {
        private static Grid _mainOverlay;

        public static void ShowImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            string absolutePath = GetAbsolutePath(imagePath);

            if (!File.Exists(absolutePath))
                return;

            BitmapImage bitmap;
            try
            {
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(absolutePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
            }
            catch
            {
                return;
            }

            var overlay = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(220, 0, 0, 0)),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            var container = new Grid();

            var fullScreenImage = new Image
            {
                Source = bitmap,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand,
                MaxWidth = SystemParameters.PrimaryScreenWidth * 0.9,
                MaxHeight = SystemParameters.PrimaryScreenHeight * 0.9
            };

            container.Children.Add(fullScreenImage);

            _mainOverlay = new Grid();
            _mainOverlay.Children.Add(overlay);
            _mainOverlay.Children.Add(container);

            _mainOverlay.MouseLeftButtonDown += (s, e) =>
            {
                if (e.OriginalSource is Image)
                    return;

                CloseImage();
            };

            _mainOverlay.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                    CloseImage();
            };

            var mainWindow = Application.Current.MainWindow as Window;
            if (mainWindow != null)
            {
                var grid = mainWindow.Content as Grid;
                if (grid != null)
                {
                    Grid.SetRowSpan(_mainOverlay, int.MaxValue);
                    Grid.SetColumnSpan(_mainOverlay, int.MaxValue);
                    grid.Children.Add(_mainOverlay);

                    _mainOverlay.Focusable = true;
                    _mainOverlay.Focus();
                    Keyboard.Focus(_mainOverlay);
                }
            }
        }

        private static string GetAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            if (Path.IsPathRooted(path))
                return path;

            if (path.StartsWith("Images/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("Images\\", StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }

        private static void CloseImage()
        {
            if (_mainOverlay != null && _mainOverlay.Parent is Grid parent)
            {
                parent.Children.Remove(_mainOverlay);
                _mainOverlay = null;
            }
        }
    }
}