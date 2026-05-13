using CozyTest.Models;
using CozyTest.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using static OfficeOpenXml.ExcelErrorValue;

namespace CozyTest.ViewModels.CreateTestsVM
{
    public class QuestionCreateViewModel : ObservableObject
    {

        private bool _isTestFinished = false;
        public bool IsTestFinished
        {
            get => _isTestFinished;
            set => SetProperty(ref _isTestFinished, value);
        }

        public Question NewQuestion { get; set; }
        public ObservableCollection<OptionCreateViewModel> Options { get; } = new();
        public ObservableCollection<OptionCreateViewModel> ConstantOptions { get; } = new();
        public ObservableCollection<OptionCreateViewModel> CorrespondingOptions { get; } = new();
        
        private readonly OptionService _optionService;

        private string? _previewImagePath;
        public string? PreviewImagePath
        {
            get => _previewImagePath;
            set => SetProperty(ref _previewImagePath, value);
        }

        public int IsQuestionType { get; set; }

        private string? _correctAnswerForTextAnswerType;
        public string? CorrectAnswerForTextAnswerType
        {
            get => _correctAnswerForTextAnswerType;
            set
            {
                SetProperty(ref _correctAnswerForTextAnswerType, value);
                UpdateCorrectTextOption();
            }
        }

        private bool _CanShow;
        public bool CanShow
        {
            get => _CanShow;
            set
            {
                _CanShow = value;
                OnPropertyChanged();
            }
        }

        private int _nextConstantNumber = 1;
        private int _nextCorrespondingNumber = 1;

        public ICommand AddConstantOptionCommand { get; }
        public ICommand AddComplianceOptionCommand { get; }

        public ICommand RemoveConstantOptionCommand { get; }
        public ICommand RemoveCorrespondingOptionCommand { get; }

        public ICommand AddTextOptionCommand { get; }
        public ICommand AddImageOptionCommand { get; }
        public ICommand SetQuestionImageCommand { get; }
        public ICommand ShowFullScreenImageCommand { get; }
        public ICommand DeleteMainImageCommand { get; }
        public bool? IsRandom
        {
            get => NewQuestion.IsRandom;
            set
            {
                NewQuestion.IsRandom = value;
                OnPropertyChanged();
            }
        }
        public string PicturePath
        {
            get => NewQuestion.PicturePath;
            set
            {
                NewQuestion.PicturePath = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(value))
                {
                    var fullPath = GetAbsoluteImagePath(value);
                    PreviewImagePath = fullPath;
                }
                else
                {
                    PreviewImagePath = null;
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

        private bool _isAnswered;
        public bool IsAnswered
        {
            get => _isAnswered;
            set
            {
                _isAnswered = value;
                OnPropertyChanged();
            }
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get => _IsSelected;
            set
            {
                _IsSelected = value;
                OnPropertyChanged();
            }
        }

        private bool _IsCorrect;
        public bool IsCorrect
        {
            get => _IsCorrect;
            set
            {
                _IsCorrect = value;
                OnPropertyChanged();
            }
        }
        public string SafeTestName => GetSafeFileName(_parent?.Test?.Name ?? "temp");

        private readonly CuratorCreateTestViewModel _parent;

        public QuestionCreateViewModel(CuratorCreateTestViewModel parent, OptionService optionService, Question? question = null)
        {
            _optionService = optionService;
            _parent = parent;
            NewQuestion = question ?? new Question() { NumberActual = parent.QuestionNumber++ };

            if (NewQuestion.QuestionTypeId is int q)
            {
                switch (q)
                {
                    case 1: IsQuestionType = 1; break;
                    case 2: IsQuestionType = 2; break;
                    case 3: IsQuestionType = 3; break;
                }
            }

            if (IsQuestionType == 1 && !string.IsNullOrEmpty(NewQuestion.PicturePath))
            {
                var fullPath = GetAbsoluteImagePath(NewQuestion.PicturePath);
                PreviewImagePath = File.Exists(fullPath) ? fullPath : null;
            }
            if (question?.QuestionTypeId == 2)
            {
                var correctOption = _optionService.GetOptionsByQuestion(question.Id).FirstOrDefault(p => p.IsCorrect == true);
                CorrectAnswerForTextAnswerType = correctOption?.TextAnswer;
            }

            AddTextOptionCommand = new RelayCommand(_ => AddTextOption(), _ => IsQuestionType == 1);
            AddImageOptionCommand = new RelayCommand(_ => AddImageOption(), _ => IsQuestionType == 1);
            AddConstantOptionCommand = new RelayCommand(_ => AddConstantOption());
            AddComplianceOptionCommand = new RelayCommand(_ => AddComplianceOption());

            SetQuestionImageCommand = new RelayCommand(_ => SetQuestionImage());
            ShowFullScreenImageCommand = new RelayCommand(_ => ShowFullScreenImage(), _ => IsQuestionType == 1);
            DeleteMainImageCommand = new RelayCommand(_ => DeleteMainImage(), _ => IsQuestionType == 1);
            RemoveConstantOptionCommand = new RelayCommand(o => RemoveConstantOption((OptionCreateViewModel)o));
            RemoveCorrespondingOptionCommand = new RelayCommand(o => RemoveCorrespondingOption((OptionCreateViewModel)o));
        }


        private void UpdateCorrectTextOption()
        {
            if (IsQuestionType == 1) return;

            var existingCorrect = Options.FirstOrDefault(o => o.IsCorrect == true);

            if (existingCorrect != null)
            {
                existingCorrect.Value = _correctAnswerForTextAnswerType ?? "";
            }
            else
            {
                var newOption = new OptionCreateViewModel(false, this, _parent)
                {
                    NewOption = new Option
                    {
                        TextAnswer = _correctAnswerForTextAnswerType ?? "",
                        IsCorrect = true,
                        IsImage = false
                    }
                };
                Options.Add(newOption);
            }
        }

        private void DeleteMainImage()
        {
            PicturePath = null;
            PreviewImagePath = null;
        }

        public void AddTextOption()
        {
            Options.Add(new OptionCreateViewModel(false, this, _parent));
        }

        public void AddImageOption()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Title = "Выберите изображение для варианта ответа",
                InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images")
            };

            if (dialog.ShowDialog() == true)
            {
                string destPath = CopyImageToTestFolder(dialog.FileName);

                if (!string.IsNullOrEmpty(destPath))
                {
                    string relativePath = GetRelativePathForTest(destPath);
                    Options.Add(new OptionCreateViewModel(true, this, _parent));
                    var newOption = Options.Last();
                    newOption.SetImagePath(relativePath);
                }
            }
        }

        public void SetQuestionImage()
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
                Title = "Выберите изображение для вопроса",
                InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images")
            };

            if (dlg.ShowDialog() == true)
            {
                string destPath = CopyImageToTestFolder(dlg.FileName);

                if (!string.IsNullOrEmpty(destPath))
                {
                    string relativePath = GetRelativePathForTest(destPath);
                    PicturePath = relativePath;
                }
            }
        }

        private string CopyImageToTestFolder(string sourcePath)
        {
            string testImagesPath = GetTestImagesFolderPath();

            if (sourcePath.StartsWith(testImagesPath, StringComparison.OrdinalIgnoreCase))
            {
                return sourcePath;
            }

            Directory.CreateDirectory(testImagesPath);

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(sourcePath)}";
            string destPath = Path.Combine(testImagesPath, fileName);

            File.Copy(sourcePath, destPath, true);

            return destPath;
        }

        private string GetTestImagesFolderPath()
        {
            string basePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Images",
                "user_tests_images",
                SafeTestName);

            return basePath;
        }

        private string GetRelativePathForTest(string absolutePath)
        {
            string testImagesPath = GetTestImagesFolderPath();

            if (absolutePath.StartsWith(testImagesPath, StringComparison.OrdinalIgnoreCase))
            {
                string fileName = Path.GetFileName(absolutePath);
                return $"{SafeTestName}/{fileName}";
            }

            return absolutePath;
        }

        public string GetAbsoluteImagePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            if (Path.IsPathRooted(relativePath))
                return relativePath;

            string normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar);

            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Images",
                "user_tests_images",
                normalizedPath);
        }

        private string GetSafeFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "temp";

            string invalidChars = new string(Path.GetInvalidFileNameChars());
            string safeName = Regex.Replace(name, $"[{Regex.Escape(invalidChars)}]", "");

            safeName = safeName.Trim();

            if (string.IsNullOrEmpty(safeName))
                return "temp";

            return safeName;
        }

        public void RemoveOption(OptionCreateViewModel option)
        {
            Options.Remove(option);
        }

        public void SetOptionImage(OptionCreateViewModel option, string path)
        {
            option.SetImagePath(path);
        }

        public void ShowFullScreenImage()
        {
            var imagePath = PreviewImagePath ?? GetAbsoluteImagePath(PicturePath);
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                FullScreenImageService.ShowImage(imagePath);
            }
        }

        //
        private void AddConstantOption()
        {
            var option = new OptionCreateViewModel(false, this, _parent)
            {
                NewOption = new Option
                {
                    Number = _nextConstantNumber++,
                    IsCorrect = false,
                    IsImage = false
                },
                IsConstant = true
            };
            ConstantOptions.Add(option);

            var optionCor = new OptionCreateViewModel(false, this, _parent)
            {
                NewOption = new Option
                {
                    Number = _nextCorrespondingNumber++,
                    IsCorrect = false,
                    IsImage = false
                },
                IsCorresponding = true,
                AvailableMatches = ConstantOptions
            };
            CorrespondingOptions.Add(optionCor);

            foreach (var corOpt in CorrespondingOptions)
            {
                corOpt.AvailableMatches = null;
                corOpt.AvailableMatches = ConstantOptions;
            }
        }

        private void AddComplianceOption()
        {
            var optionCor = new OptionCreateViewModel(false, this, _parent)
            {
                NewOption = new Option
                {
                    Number = _nextCorrespondingNumber++,
                    IsCorrect = false,
                    IsImage = false
                },
                IsCorresponding = true
            };
            Options.Add(optionCor);
        }
        private void RemoveConstantOption(OptionCreateViewModel option)
        {
            int index = ConstantOptions.IndexOf(option);

            if (index >= 0 && index < CorrespondingOptions.Count)
            {
                var correspondingToRemove = CorrespondingOptions[index];
                CorrespondingOptions.RemoveAt(index);
            }

            ConstantOptions.Remove(option);

            RenumberConstants();
            RenumberCorrespondings();

            foreach (var corOpt in CorrespondingOptions)
            {
                corOpt.AvailableMatches = null;
                corOpt.AvailableMatches = ConstantOptions;
            }
        }

        private void RemoveCorrespondingOption(OptionCreateViewModel option)
        {
            CorrespondingOptions.Remove(option);
            RenumberCorrespondings();
        }

        private void RenumberConstants()
        {
            int num = 1;
            foreach (var opt in ConstantOptions)
            {
                opt.NewOption.Number = num++;
            }
            _nextConstantNumber = num;
        }

        private void RenumberCorrespondings()
        {
            int num = 1;
            foreach (var opt in CorrespondingOptions)
            {
                opt.NewOption.Number = num++;
            }
            _nextCorrespondingNumber = num;
        }


        public void RemoveComplianceOption(OptionCreateViewModel option)
        {
            Options.Remove(option);
        }
    }
}