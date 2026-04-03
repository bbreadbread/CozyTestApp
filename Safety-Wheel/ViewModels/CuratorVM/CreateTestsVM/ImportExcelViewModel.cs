using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CreateTestsVM;
using Microsoft.Win32;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace CozyTest.ViewModels.CuratorVM.CreateTestsVM
{
    public class ImportExcelViewModel : BaseViewModel
    {
        public override string WindowTitle => "Еастройка импорта теста из файла";

        private readonly TopicService _topicService = new();
        private readonly TestService _testService = new();
        private readonly QuestionService _questionService = new();
        private readonly OptionService _optionService = new();
        private readonly IDialogService _dialogService;

        public ObservableCollection<Topic> Topics => _topicService.Topics;

        private Topic? _selectedTopic;
        public Topic? SelectedTopic
        {
            get => _selectedTopic;
            set
            {
                _selectedTopic = value;
                OnPropertyChanged();
            }
        }

        private bool _isTestType = true;
        public bool IsTestType
        {
            get => _isTestType;
            set
            {
                _isTestType = value;
                OnPropertyChanged();
            }
        }

        private bool _isSurveyType;
        public bool IsSurveyType
        {
            get => _isSurveyType;
            set
            {
                _isSurveyType = value;
                OnPropertyChanged();
            }
        }

        private string? _selectedFilePath;
        public string? SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                _selectedFilePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedFileName));
            }
        }

        public string SelectedFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_selectedFilePath))
                    return "Файл не выбран";
                return Path.GetFileName(_selectedFilePath);
            }
        }

        public ICommand OpenFileDialogCommand { get; }
        public ICommand DownloadExampleCommand { get; }
        public ICommand ImportCommand { get; }

        public ImportExcelViewModel(
            INavigationService navigationService,
            IDialogService dialogService) : base(navigationService, dialogService)
        {
            _dialogService = dialogService;
            _topicService.GetAll();

            OpenFileDialogCommand = new RelayCommand(_ => OpenFileDialog());
            DownloadExampleCommand = new RelayCommand(_ => DownloadExample());
            ImportCommand = new RelayCommand(_ => ImportTest(), _ => CanImport());
        }

        private void OpenFileDialog()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                Title = "Выберите файл для импорта"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedFilePath = openFileDialog.FileName;
            }
        }

        private void DownloadExample()
        {
            try
            {
                string sourcePath = @"D:\VisualStudioProject\CozyTest\Safety-Wheel\bin\Debug\net8.0-windows\Пример теста для импорта.xlsx";

                if (!File.Exists(sourcePath))
                {
                    _dialogService.ShowMessage("Файл примера не найден", "Ошибка");
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    FileName = "Пример теста для импорта.xlsx",
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    Title = "Сохранить пример"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.Copy(sourcePath, saveFileDialog.FileName, overwrite: true);
                    _dialogService.ShowMessage("Пример успешно сохранён", "Готово");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка при сохранении: {ex.Message}", "Ошибка");
            }
        }

        private bool CanImport()
        {
            return SelectedTopic != null && !string.IsNullOrEmpty(_selectedFilePath) && File.Exists(_selectedFilePath);
        }

        private void ImportTest()
        {
            if (SelectedTopic == null)
            {
                _dialogService.ShowMessage("Выберите тему теста", "Ошибка");
                return;
            }

            if (string.IsNullOrEmpty(_selectedFilePath) || !File.Exists(_selectedFilePath))
            {
                _dialogService.ShowMessage("Выберите файл для импорта", "Ошибка");
                return;
            }

            try
            {
                ExcelPackage.License.SetNonCommercialPersonal("CozyTest");

                using var package = new ExcelPackage(new FileInfo(_selectedFilePath));
                var worksheet = package.Workbook.Worksheets[0];

                if (worksheet == null)
                {
                    _dialogService.ShowMessage("Excel файл пуст или повреждён", "Ошибка");
                    return;
                }

                var validationResult = ValidateExcelFile(worksheet);
                if (!validationResult.IsValid)
                {
                    _dialogService.ShowMessage(validationResult.ErrorMessage, "Ошибка в файле");
                    return;
                }

                var test = ParseTestFromExcel(worksheet);

                if (test.Questions.Count == 0)
                {
                    _dialogService.ShowMessage("В файле не найдено вопросов", "Ошибка");
                    return;
                }

                test.TopicId = SelectedTopic.Id;
                test.TestTypeId = IsTestType ? 1 : 2;
                test.CuratorCreateId = CurrentUser.Id;
                test.DateOfCreating = DateTime.Now;
                test.IsArchive = false;

                _testService.Add(test, test.Questions.Count);

                var createdTest = _testService.GetLastTest();

                _dialogService.ShowMessage($"Тест \"{test.Name}\" успешно импортирован.", "Успех");
                _dialogService.CloseWindow(this);
                _navigationService.NavigateTo(new CuratorCreateTestViewModel(_navigationService, _dialogService, createdTest));
               
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка при импорте: {ex.Message}", "Ошибка");
            }
        }

        private ValidationResult ValidateExcelFile(ExcelWorksheet worksheet)
        {
            var result = new ValidationResult { IsValid = true };

            int rowCount = worksheet.Dimension?.Rows ?? 0;
            if (rowCount < 3)
            {
                result.IsValid = false;
                result.ErrorMessage = "Файл слишком короткий. Минимум 3 строки (заголовок, описание, данные).";
                return result;
            }

            string? testName = worksheet.Cells[2, 1].Text?.Trim();
            if (string.IsNullOrEmpty(testName))
            {
                result.IsValid = false;
                result.ErrorMessage = "Не указано название теста (столбец A, строка 2).";
                return result;
            }

            var questionNumbers = new HashSet<int>();
            int currentRow = 2;
            bool hasQuestions = false;

            while (currentRow <= rowCount)
            {
                string? questionNumStr = worksheet.Cells[currentRow, 2].Text?.Trim();
                string? questionTypeStr = worksheet.Cells[currentRow, 3].Text?.Trim();
                string? questionText = worksheet.Cells[currentRow, 4].Text?.Trim();

                if (string.IsNullOrEmpty(questionNumStr))
                {
                    currentRow++;
                    continue;
                }

                if (!int.TryParse(questionNumStr, out int questionNum) || questionNum <= 0)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Строка {currentRow}: Некорректный номер вопроса.";
                    return result;
                }

                if (!questionNumbers.Contains(questionNum))
                {
                    questionNumbers.Add(questionNum);
                    hasQuestions = true;

                    if (string.IsNullOrEmpty(questionTypeStr) || (questionTypeStr != "1" && questionTypeStr != "2"))
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Строка {currentRow}: Тип вопроса должен быть 1 (текст) или 2 (изображение).";
                        return result;
                    }

                    if (string.IsNullOrEmpty(questionText))
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Строка {currentRow}: Отсутствует текст вопроса №{questionNum}.";
                        return result;
                    }

                }

                string? answerText = worksheet.Cells[currentRow, 6].Text?.Trim();
                if (string.IsNullOrEmpty(answerText))
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Строка {currentRow}: Отсутствует текст ответа.";
                    return result;
                }

                currentRow++;
            }

            foreach (var qNum in questionNumbers.OrderBy(x => x))
            {
                var questionRows = new List<int>();
                for (int r = 2; r <= rowCount; r++)
                {
                    if (worksheet.Cells[r, 2].Text?.Trim() == qNum.ToString())
                        questionRows.Add(r);
                }

                if (questionRows.Count < 2)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Вопрос №{qNum} должен иметь минимум 2 варианта ответа.";
                    return result;
                }

                bool hasCorrectAnswer = false;
                foreach (var row in questionRows)
                {
                    string? isCorrect = worksheet.Cells[row, 7].Text?.Trim();
                    if (isCorrect == "1")
                    {
                        hasCorrectAnswer = true;
                        break;
                    }
                }

                if (!hasCorrectAnswer)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Вопрос №{qNum} должен иметь хотя бы один правильный ответ (1 в столбце G).";
                    return result;
                }
            }

            if (!hasQuestions)
            {
                result.IsValid = false;
                result.ErrorMessage = "В файле не найдено ни одного вопроса.";
                return result;
            }

            return result;
        }

        private Test ParseTestFromExcel(ExcelWorksheet worksheet)
        {
            var test = new Test
            {
                Name = worksheet.Cells[3, 1].Text?.Trim() ?? "Без названия",
                Description = $"Импортировано из Excel: {Path.GetFileName(_selectedFilePath)}",
                Questions = new List<Question>()
            };

            int rowCount = worksheet.Dimension.Rows;
            int currentRow = 2;
            int numOpt = 1;

            var questionsDict = new Dictionary<int, Question>();

            while (currentRow <= rowCount)
            {
                string? questionNumStr = worksheet.Cells[currentRow, 2].Text?.Trim();
                if (string.IsNullOrEmpty(questionNumStr) || !int.TryParse(questionNumStr, out int questionNum))
                {
                    currentRow++;
                    continue;
                }

                string? questionTypeStr = worksheet.Cells[currentRow, 3].Text?.Trim();
                string? questionText = worksheet.Cells[currentRow, 4].Text?.Trim();
                string? imagePath = worksheet.Cells[currentRow, 5].Text?.Trim();
                string? answerText = worksheet.Cells[currentRow, 6].Text?.Trim();
                string? isCorrectStr = worksheet.Cells[currentRow, 7].Text?.Trim();

                if (!questionsDict.ContainsKey(questionNum))
                {
                    numOpt = 1;
                    var question = new Question
                    {
                        TestQuest = questionText,
                        QuestionTypeId = int.Parse(questionTypeStr),
                        PicturePath = questionTypeStr == "1" ? imagePath : null,
                        Options = new List<Option>(),
                        Version = 1,
                        IsArchive = false
                    };
                    questionsDict[questionNum] = question;
                }

                var option = new Option
                {
                    Question = questionsDict[questionNum],
                    Number = numOpt,
                    TextAnswer = answerText,
                    IsCorrect = isCorrectStr == "1",
                    IsArchive = false
                };

                questionsDict[questionNum].Options.Add(option);

                numOpt++;
                currentRow++;
            }

            test.Questions = questionsDict.OrderBy(x => x.Key).Select(x => x.Value).ToList();
            test.PenaltyMax = test.Questions.Count;

            return test;
        }

        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; } = string.Empty;
        }
    }
}