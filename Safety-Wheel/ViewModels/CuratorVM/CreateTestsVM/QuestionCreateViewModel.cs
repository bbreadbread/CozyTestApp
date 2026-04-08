using CozyTest.Models;
using CozyTest.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace CozyTest.ViewModels.CreateTestsVM
{
    public class QuestionCreateViewModel : ObservableObject
    {
        public Question NewQuestion { get; set; }
        public ObservableCollection<OptionCreateViewModel> Options { get; } = new();

        private string? _previewImagePath;
        public string? PreviewImagePath
        {
            get => _previewImagePath;
            set => SetProperty(ref _previewImagePath, value);
        }

        public ICommand AddTextOptionCommand { get; }
        public ICommand AddImageOptionCommand { get; }
        public ICommand SetQuestionImageCommand { get; }
        public ICommand ShowFullScreenImageCommand { get; }
        public string PicturePath
        {
            get => NewQuestion.PicturePath;
            set
            {
                NewQuestion.PicturePath = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(value))
                {
                    var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, value);
                    PreviewImagePath = fullPath;
                }
            }
        }

        public string Text
        {
            get => NewQuestion.TestQuest ?? "";
            set
            {
                NewQuestion.TestQuest = value;
                OnPropertyChanged();
            }
        }

        public string Comments
        {
            get => NewQuestion.Comments ?? "";
            set
            {
                NewQuestion.Comments = value;
                OnPropertyChanged();
            }
        }

        public QuestionCreateViewModel(Question? question = null)
        {
            NewQuestion = question ?? new Question();
            if (!string.IsNullOrEmpty(NewQuestion.PicturePath))
            {
                var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NewQuestion.PicturePath);
                PreviewImagePath = File.Exists(fullPath) ? fullPath : null;
            }
            AddTextOptionCommand = new RelayCommand(_ => AddTextOption());
            AddImageOptionCommand = new RelayCommand(_ => AddImageOption());
            SetQuestionImageCommand = new RelayCommand(_ => SetQuestionImage());
            ShowFullScreenImageCommand = new RelayCommand(_ => ShowFullScreenImage());
        }

        public void AddTextOption()
        {
            Options.Add(new OptionCreateViewModel(false, this));
        }

        public void AddImageOption()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.gif",
                Title = "Выберите изображение для варианта ответа"
            };

            if (dialog.ShowDialog() == true)
            {
                string imagesPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                string relativePath;

                if (dialog.FileName.StartsWith(imagesPath, StringComparison.OrdinalIgnoreCase))
                {
                    relativePath = dialog.FileName.Substring(imagesPath.Length + 1);
                }
                else
                {
                    string fileName = $"{Guid.NewGuid()}{System.IO.Path.GetExtension(dialog.FileName)}";
                    string destPath = System.IO.Path.Combine(imagesPath, fileName);
                    Directory.CreateDirectory(imagesPath);
                    File.Copy(dialog.FileName, destPath, true);
                    relativePath = fileName;
                }

                relativePath = relativePath.Replace('\\', '/');

                Options.Add(new OptionCreateViewModel(true, this));

                var newOption = Options.Last();
                newOption.SetImagePath("Images/" + relativePath);
            }
        }

        public void RemoveOption(OptionCreateViewModel option)
        {
            Options.Remove(option);
            RecalculateQuestionType();
        }

        public void RecalculateQuestionType()
        {
            int correctCount = Options.Count(o => o.IsCorrect == true);
            NewQuestion.QuestionTypeId = correctCount <= 1 ? 1 : 3;
        }

        public void SetQuestionImage()
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp",
                InitialDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images")
            };

            if (dlg.ShowDialog() == true)
            {
                string imagesPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                string relativePath;

                if (dlg.FileName.StartsWith(imagesPath, StringComparison.OrdinalIgnoreCase))
                {
                    relativePath = dlg.FileName.Substring(imagesPath.Length + 1);
                }
                else
                {
                    string fileName = $"{Guid.NewGuid()}{System.IO.Path.GetExtension(dlg.FileName)}";
                    string destPath = System.IO.Path.Combine(imagesPath, fileName);
                    Directory.CreateDirectory(imagesPath);
                    File.Copy(dlg.FileName, destPath, true);
                    relativePath = fileName;
                }

                relativePath = relativePath.Replace('\\', '/');

                PicturePath = "Images/" + relativePath;
            }
        }

        public void SetOptionImage(OptionCreateViewModel option, string path)
        {
            option.SetImagePath(path);
        }

        public void ShowFullScreenImage()
        {
            var imagePath = PreviewImagePath ?? PicturePath;
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                FullScreenImageService.ShowImage(imagePath);
            }
        }
    }
}