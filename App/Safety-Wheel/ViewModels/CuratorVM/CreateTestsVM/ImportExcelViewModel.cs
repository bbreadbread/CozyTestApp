using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CreateTestsVM;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace CozyTest.ViewModels.CuratorVM.CreateTestsVM
{
    public class ImportExcelViewModel : BaseViewModel
    {
        public override string WindowTitle => "Настройка импорта теста из файла";

        private readonly TopicService _topicService;
        private readonly TestService _testService;
        private readonly QuestionService _questionService;
        private readonly OptionService _optionService;
        private CriteriaService _criteriaService;
        private readonly CorrespondenceService _correspondenceService;
        private readonly IDialogService _dialogService;
        private readonly IServiceProvider _serviceProvider;

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

        private int _MaxAttemptsPerPerson;
        public int MaxAttemptsPerPerson
        {
            get => _MaxAttemptsPerPerson;
            set
            {
                _MaxAttemptsPerPerson = value;
                OnPropertyChanged();
            }
        }

        private int _TimeLimitSecond;
        public int TimeLimitSecond
        {
            get => _TimeLimitSecond;
            set
            {
                _TimeLimitSecond = value;
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
               IDialogService dialogService,
               TopicService topicService,
               TestService testService,
               QuestionService questionService,
               CriteriaService criteriaService,
               OptionService optionService,
               CorrespondenceService correspondenceService,
               IServiceProvider serviceProvider, ILoggingService logger) : base(navigationService, dialogService, logger)
        {
            _dialogService = dialogService;
            _topicService = topicService;
            _testService = testService;
            _questionService = questionService;
            _optionService = optionService;
            _correspondenceService = correspondenceService;
            _serviceProvider = serviceProvider;
            _criteriaService = criteriaService;

            _ = InitializeAsync();

            OpenFileDialogCommand = new RelayCommand(_ => OpenFileDialog());
            DownloadExampleCommand = new RelayCommand(_ => DownloadExample());
            ImportCommand = new RelayCommand(_ => _ = ImportTestAsync(), _ => CanImport());
        }

        private async Task InitializeAsync()
        {
            await _topicService.InitializeAsync();
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

        private async Task ImportTestAsync()
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

                var testData = ParseTestFromExcel(worksheet);

                if (testData.Questions.Count == 0)
                {
                    _dialogService.ShowMessage("В файле не найдено вопросов", "Ошибка");
                    return;
                }

                Test test = testData.Test;
                test.TopicId = SelectedTopic.Id;
                test.TestTypeId = IsTestType ? 1 : 2;
                test.CuratorCreateId = CurrentUser.Id;
                test.DateOfCreating = DateTime.Now;
                test.IsArchive = false;
                test.TimeLimitSecond = TimeLimitSecond;
                test.MaxNumPassing = MaxAttemptsPerPerson;

                await _testService.AddAsync(test, test.Questions.Count);

                await _criteriaService.AddAsync(new Criteria { TestId = test.Id, Name = "5 (отлично)", MinPercent = 85, IsActive = true, OrderNumber = 5 });
                await _criteriaService.AddAsync(new Criteria { TestId = test.Id, Name = "4 (хорошо)", MinPercent = 70, IsActive = true, OrderNumber = 4 });
                await _criteriaService.AddAsync(new Criteria { TestId = test.Id, Name = "3 (удовлетворительно)", MinPercent = 50, IsActive = true, OrderNumber = 3 });
                await _criteriaService.AddAsync(new Criteria { TestId = test.Id, Name = "2 (неудовлетворительно)", MinPercent = 0, IsActive = true, OrderNumber = 2 });
                await _criteriaService.AddAsync(new Criteria { TestId = test.Id, Name = "1 (неявка/незачёт)", MinPercent = 0, IsActive = true, OrderNumber = 1 });

                var createdTest = await _testService.GetLastTestAsync();

                int qi = 1;
                foreach (var questionData in testData.Questions)
                {
                    questionData.Question.TestId = createdTest.Id;
                    questionData.Question.NumberActual = qi++;

                    var savedQuestion = await _questionService.AddAsync(questionData.Question, createdTest, questionData.Question.NumberActual);

                    if (questionData.QuestionType == 3)
                    {
                        var correspondences = new List<(int constantId, int correspondingId)>();

                        var savedConstants = new List<Option>();
                        int ci = 1;
                        foreach (var constantOpt in questionData.ConstantOptions.OrderBy(o => o.Number))
                        {
                            constantOpt.QuestionId = savedQuestion.Id;
                            constantOpt.Number = ci++;
                            constantOpt.IsCorrect = null;
                            var savedConstant = await _optionService.AddReturnAsync(constantOpt, constantOpt.Number);
                            savedConstants.Add(savedConstant);
                        }

                        var savedOptions = new List<Option>();
                        int ri = 1;
                        foreach (var opt in questionData.Options)
                        {
                            opt.QuestionId = savedQuestion.Id;
                            opt.Number = ri++;
                            opt.IsCorrect = null;
                            var savedOption = await _optionService.AddReturnAsync(opt, opt.Number);
                            savedOptions.Add(savedOption);
                        }

                        for (int i = 0; i < questionData.ConstantOptions.Count && i < questionData.Correspondences.Count; i++)
                        {
                            var constantOpt = questionData.ConstantOptions[i];
                            var correspondence = questionData.Correspondences[i];

                            var savedConstant = savedConstants.FirstOrDefault(c => c.Number == constantOpt.Number);
                            var savedCorresponding = savedOptions.FirstOrDefault(o => o.Number == correspondence.correspondingNumber);

                            if (savedConstant != null && savedCorresponding != null)
                            {
                                correspondences.Add((savedConstant.Id, savedCorresponding.Id));
                            }
                        }

                        if (correspondences.Any())
                        {
                            await _correspondenceService.SaveForQuestionAsync(savedQuestion.Id, correspondences);
                        }
                    }
                    else if (questionData.QuestionType == 1)
                    {
                        int oi = 1;
                        foreach (var opt in questionData.Options)
                        {
                            opt.QuestionId = savedQuestion.Id;
                            opt.Number = oi++;
                            await _optionService.AddAsync(opt, opt.Number);
                        }
                    }
                    else if (questionData.QuestionType == 2)
                    {
                        int oi = 1;
                        foreach (var opt in questionData.Options)
                        {
                            opt.QuestionId = savedQuestion.Id;
                            opt.Number = oi++;
                            opt.IsCorrect = true;
                            await _optionService.AddAsync(opt, opt.Number);
                        }
                    }
                }

                _dialogService.ShowMessage($"Тест \"{test.Name}\" успешно импортирован.", "Успех");
                _dialogService.CloseWindow(this);

                var viewModel = ActivatorUtilities.CreateInstance<CuratorCreateTestViewModel>(
                    _serviceProvider,
                    test);
                _navigationService.NavigateTo(viewModel);
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
            var questionTypes = new Dictionary<int, int>();

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

                    if (string.IsNullOrEmpty(questionTypeStr) ||
                        (questionTypeStr != "1" && questionTypeStr != "2" && questionTypeStr != "3"))
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Строка {currentRow}: Тип вопроса должен быть 1, 2 или 3.";
                        return result;
                    }

                    if (string.IsNullOrEmpty(questionText))
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Строка {currentRow}: Отсутствует текст вопроса №{questionNum}.";
                        return result;
                    }

                    questionTypes[questionNum] = int.Parse(questionTypeStr);
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

                int questionType = questionTypes[qNum];

                if (questionType == 1 || questionType == 2)
                {
                    if (questionRows.Count < 1)
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Вопрос №{qNum} должен иметь минимум 1 варианта ответа.";
                        return result;
                    }

                    bool hasCorrectAnswer = false;
                    foreach (var row in questionRows)
                    {
                        string? isCorrect = worksheet.Cells[row, 8].Text?.Trim();
                        if (isCorrect == "1")
                        {
                            hasCorrectAnswer = true;
                            break;
                        }
                    }

                    if (questionType == 1 && !hasCorrectAnswer)
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Вопрос №{qNum} должен иметь хотя бы один правильный ответ (1 в столбце H).";
                        return result;
                    }
                }
                else if (questionType == 3)
                {
                    var constantParts = new HashSet<string>();
                    var correspondingNumbers = new HashSet<int?>();

                    foreach (var row in questionRows)
                    {
                        string? constantPart = worksheet.Cells[row, 5].Text?.Trim();
                        string? answerText = worksheet.Cells[row, 6].Text?.Trim();
                        string? correctOrder = worksheet.Cells[row, 8].Text?.Trim();

                        if (string.IsNullOrEmpty(answerText))
                        {
                            result.IsValid = false;
                            result.ErrorMessage = $"Строка {row}: Отсутствует текст варианта ответа для вопроса №{qNum}.";
                            return result;
                        }

                        if (!string.IsNullOrEmpty(constantPart))
                        {
                            constantParts.Add(constantPart);
                        }

                        if (!string.IsNullOrEmpty(correctOrder) && int.TryParse(correctOrder, out int order))
                        {
                            correspondingNumbers.Add(order);
                        }
                    }

                    if (constantParts.Count == 0)
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Вопрос №{qNum} типа 3 должен иметь хотя бы одну постоянную часть (столбец E).";
                        return result;
                    }

                    foreach (var row in questionRows)
                    {
                        string? correctOrder = worksheet.Cells[row, 7].Text?.Trim();
                        string? constantPart = worksheet.Cells[row, 5].Text?.Trim();

                        if (!string.IsNullOrEmpty(correctOrder) && string.IsNullOrEmpty(constantPart))
                        {
                            result.IsValid = false;
                            result.ErrorMessage = $"Строка {row}: Для соответствия указан порядковый номер, но не указана постоянная часть.";
                            return result;
                        }
                    }
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

        private TestImportData ParseTestFromExcel(ExcelWorksheet worksheet)
        {
            string testName = worksheet.Cells[2, 1].Text?.Trim() ?? "Без названия";
            string safeTestName = GetSafeFileName(testName);

            var test = new Test
            {
                Name = testName,
                Description = $"Импортировано из Excel: {Path.GetFileName(_selectedFilePath)}",
                PenaltyMax = 0
            };

            int rowCount = worksheet.Dimension.Rows;
            int currentRow = 2;

            var questionsData = new Dictionary<int, QuestionImportData>();

            while (currentRow <= rowCount)
            {
                string? questionNumStr = worksheet.Cells[currentRow, 2].Text?.Trim();
                if (string.IsNullOrEmpty(questionNumStr) || !int.TryParse(questionNumStr, out int questionNum))
                {
                    currentRow++;
                    continue;
                }

                if (!questionsData.ContainsKey(questionNum))
                {
                    string? questionTypeStr = worksheet.Cells[currentRow, 3].Text?.Trim();
                    string? questionText = worksheet.Cells[currentRow, 4].Text?.Trim();
                    string? mainImagePath = worksheet.Cells[currentRow, 5].Text?.Trim();

                    int questionType = int.Parse(questionTypeStr);

                    var question = new Question
                    {
                        TestQuest = questionText,
                        QuestionTypeId = questionType,
                        PicturePath = !string.IsNullOrEmpty(mainImagePath) ? GetSafeImagePath(mainImagePath, safeTestName) : null,
                        Options = new List<Option>()
                    };

                    questionsData[questionNum] = new QuestionImportData
                    {
                        Question = question,
                        QuestionType = questionType,
                        Options = new List<Option>(),
                        ConstantOptions = new List<Option>()
                    };
                }

                var questionData = questionsData[questionNum];
                string? constantPart = worksheet.Cells[currentRow, 5].Text?.Trim();
                string? answerText = worksheet.Cells[currentRow, 6].Text?.Trim();
                string? optionIsImage = worksheet.Cells[currentRow, 7].Text?.Trim();
                string? correctOrder = worksheet.Cells[currentRow, 8].Text?.Trim();

                if (questionData.QuestionType == 3)
                {
                    if (!string.IsNullOrEmpty(constantPart))
                    {
                        int constNum = 1;
                        var existingConstant = questionData.ConstantOptions
                            .FirstOrDefault(o => o.TextAnswer == constantPart);

                        if (existingConstant != null)
                        {
                            constNum = existingConstant.Number;
                        }
                        else
                        {
                            constNum = questionData.ConstantOptions.Count + 1;
                            var constantOption = new Option
                            {
                                TextAnswer = constantPart,
                                Number = constNum,
                                IsCorrect = null,
                                IsConstant = true,
                                IsImage = false
                            };
                            questionData.ConstantOptions.Add(constantOption);
                        }

                        if (!string.IsNullOrEmpty(answerText))
                        {
                            int order = 1;
                            if (!string.IsNullOrEmpty(correctOrder) && int.TryParse(correctOrder, out int parsedOrder))
                            {
                                order = parsedOrder;
                            }
                            else
                            {
                                var maxOrder = questionData.Options
                                    .Where(o => o.CorrespondingNumber == constNum)
                                    .Select(o => o.Number)
                                    .DefaultIfEmpty(0)
                                    .Max();
                                order = (int)maxOrder + 1;
                            }

                            Option existingCorresponding = questionData.Options
                                .FirstOrDefault(o => o.TextAnswer == answerText);

                            if (existingCorresponding != null)
                            {
                                if (!questionData.Correspondences.Any(c => c.constantNumber == constNum && c.correspondingNumber == existingCorresponding.Number))
                                {
                                    questionData.Correspondences.Add((constNum, existingCorresponding.Number));
                                }
                            }
                            else
                            {
                                var correspondingOption = new Option
                                {
                                    TextAnswer = answerText,
                                    Number = order,
                                    CorrespondingNumber = constNum,
                                    IsCorrect = null,
                                    IsImage = optionIsImage == "1"
                                };
                                questionData.Options.Add(correspondingOption);
                                questionData.Correspondences.Add((constNum, order));
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(answerText))
                    {
                        var regularOption = new Option
                        {
                            TextAnswer = answerText,
                            Number = questionData.Options.Count + 1,
                            CorrespondingNumber = null,
                            IsCorrect = null,
                            IsImage = optionIsImage == "1"
                        };
                        questionData.Options.Add(regularOption);
                    }
                }
                else
                {
                    bool isCorrect = correctOrder == "1";

                    var option = new Option
                    {
                        TextAnswer = answerText,
                        Number = questionData.Options.Count + 1,
                        IsCorrect = isCorrect,
                        IsImage = optionIsImage == "1"
                    };

                    questionData.Options.Add(option);
                }

                currentRow++;
            }

            var result = new TestImportData
            {
                Test = test,
                Questions = questionsData.OrderBy(x => x.Key).Select(x => x.Value).ToList()
            };

            test.PenaltyMax = result.Questions.Count;

            return result;
        }

        private string GetSafeImagePath(string pathFromExcel, string safeTestName)
        {
            if (string.IsNullOrEmpty(pathFromExcel))
                return pathFromExcel;

            string normalizedPath = pathFromExcel.Replace('\\', '/');

            if (normalizedPath.StartsWith(safeTestName + "/", StringComparison.OrdinalIgnoreCase))
            {
                return normalizedPath;
            }

            if (normalizedPath.Contains('/'))
            {
                return normalizedPath;
            }

            return $"{safeTestName}/{normalizedPath}";
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

        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; } = string.Empty;
        }

        private class QuestionImportData
        {
            public Question Question { get; set; } = new Question();
            public int QuestionType { get; set; }
            public List<Option> Options { get; set; } = new List<Option>();
            public List<Option> ConstantOptions { get; set; } = new List<Option>();
            public List<(int constantNumber, int correspondingNumber)> Correspondences { get; set; } = new List<(int, int)>();
        }

        private class TestImportData
        {
            public Test Test { get; set; } = new Test();
            public List<QuestionImportData> Questions { get; set; } = new List<QuestionImportData>();
        }
    }
}