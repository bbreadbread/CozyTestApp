using CozyTest.Models;
using CozyTest.Services;
using System.Windows.Input;
using System.IO;

namespace CozyTest.ViewModels.CreateTestsVM
{
    public class OptionCreateViewModel : ObservableObject
    {
        public Option NewOption { get; set; } = new();

        // Тип ответа: true = изображение, false = текст
        public bool IsImage { get; set; }

        private readonly QuestionCreateViewModel _parent;

        // Свойства для позиционирования в Grid
        private int _rowIndex;
        public int RowIndex
        {
            get => _rowIndex;
            set => SetProperty(ref _rowIndex, value);
        }

        private int _columnIndex;
        public int ColumnIndex
        {
            get => _columnIndex;
            set => SetProperty(ref _columnIndex, value);
        }

        private int _columnSpan = 1;
        public int ColumnSpan
        {
            get => _columnSpan;
            set => SetProperty(ref _columnSpan, value);
        }

        public string Value
        {
            get => NewOption.TextAnswer ?? "";
            set
            {
                NewOption.TextAnswer = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AbsoluteImagePath)); // Уведомляем об изменении абсолютного пути
            }
        }

        // Свойство для получения абсолютного пути к изображению
        public string AbsoluteImagePath => GetAbsoluteImagePath(Value);

        public bool? IsCorrect
        {
            get => NewOption.IsCorrect;
            set
            {
                NewOption.IsCorrect = value;
                OnPropertyChanged();
                _parent.RecalculateQuestionType();
            }
        }

        public ICommand DeleteCommand { get; }
        public ICommand ShowFullScreenImageCommand { get; }

        public OptionCreateViewModel(bool isImage, QuestionCreateViewModel parent)
        {
            IsImage = isImage;
            _parent = parent;
            DeleteCommand = new RelayCommand(_ => _parent.RemoveOption(this));
            ShowFullScreenImageCommand = new RelayCommand(_ => ShowFullScreenImage());
        }

        public void SetImagePath(string path)
        {
            Value = path;
        }

        private void ShowFullScreenImage()
        {
            if (!IsImage || string.IsNullOrEmpty(Value))
                return;

            string absolutePath = GetAbsoluteImagePath(Value);

            if (File.Exists(absolutePath))
            {
                FullScreenImageService.ShowImage(absolutePath);
            }
        }

        private string GetAbsoluteImagePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            // Если путь уже абсолютный
            if (Path.IsPathRooted(path))
                return path;

            // Заменяем слеши на правильные для текущей ОС
            path = path.Replace('/', Path.DirectorySeparatorChar);

            // Если путь начинается с Images/
            if (path.StartsWith("Images" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            }

            // Для остальных относительных путей
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }
    }
}