using CozyTest.Models;
using CozyTest.Pages.Curator;
using CozyTest.Services;
using CozyTest.ViewModels.CuratorVM;
using CozyTest.ViewModels.CuratorVM.CreateTestsVM;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CozyTest.ViewModels.CreateTestsVM
{
    public class CuratorCreateTestViewModel : BaseViewModel
    {
        private readonly CorrespondenceService _correspondenceService;
        private CriteriaService _criteriaService;
        private int _qn;
        private int _currentVersion;

        private Test _originalTest;
        private List<Question> _originalQuestions;
        private List<Option> _originalOptions;
        private List<Сorrespondence> _originalCorrespondences;

        public int QuestionNumber
        {
            get => _qn;
            set => SetProperty(ref _qn, value);
        }

        public bool IsEditMode { get; }

        private Test _test;
        public Test Test
        {
            get => _test;
            set
            {
                _test = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TestName));
                OnPropertyChanged(nameof(TestDescription));
                OnPropertyChanged(nameof(TestTimeLimit));
                OnPropertyChanged(nameof(TestMaxNumPassing));
                OnPropertyChanged(nameof(TestIsRandom));
                OnPropertyChanged(nameof(HasQuestions));
            }
        }

        public string TestName
        {
            get => Test?.Name ?? string.Empty;
            set
            {
                if (Test != null)
                {
                    Test.Name = value;
                    OnPropertyChanged();
                }
            }
        }
        public string TestDescription
        {
            get => Test?.Description ?? string.Empty;
            set
            {
                if (Test != null)
                {
                    Test.Description = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool HasQuestions => Questions != null && Questions.Any();
        public bool? TestIsRandom
        {
            get => Test.IsRandom;
            set
            {
                Test.IsRandom = value;
                OnPropertyChanged();
            }
        }
        public bool? TestIsShowNowAnswer
        {
            get => Test.IsShowNowAnswer;
            set
            {
                Test.IsShowNowAnswer = value;
                OnPropertyChanged();
            }
        }
        public int TestTimeLimit
        {
            get => Test?.TimeLimitSecond ?? 0;
            set
            {
                if (Test != null)
                {
                    Test.TimeLimitSecond = value;
                    OnPropertyChanged();
                }
            }
        }
        public int TestMaxNumPassing
        {
            get => Test?.MaxNumPassing ?? 0;
            set
            {
                if (Test != null)
                {
                    Test.MaxNumPassing = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _isTestFinished = true;
        public bool IsTestFinished
        {
            get => _isTestFinished;
            set => SetProperty(ref _isTestFinished, value);
        }
        public ObservableCollection<QuestionCreateViewModel> Questions { get; } = new();

        private readonly TopicService _topicService;
        private readonly DTestTypeService _dTestTypeService;

        private ObservableCollection<Topic> _topics = new();
        public ObservableCollection<Topic> Topics
        {
            get => _topics;
            set => SetProperty(ref _topics, value);
        }

        private ObservableCollection<DTestType> _dTestTypes = new();
        public ObservableCollection<DTestType> DTestTypes
        {
            get => _dTestTypes;
            set => SetProperty(ref _dTestTypes, value);
        }

        private Topic? _selectedTopic;
        public Topic? SelectedTopic
        {
            get => _selectedTopic;
            set
            {
                _selectedTopic = value;
                OnPropertyChanged();
                if (value != null && Test != null) Test.TopicId = value.Id;
            }
        }

        private DTestType? _selectedDTestType;
        public DTestType? SelectedDTestType
        {
            get => _selectedDTestType;
            set
            {
                _selectedDTestType = value;
                OnPropertyChanged();
                if (value != null && Test != null) Test.TestTypeId = value.Id;
            }
        }

        public ICommand AddQuestionCommand { get; }
        public ICommand AddTextQuestionCommand { get; }
        public ICommand AddComplianceQuestionCommand { get; }
        public ICommand RemoveQuestionCommand { get; }
        public ICommand SaveTestCommand { get; }
        public ICommand SelectQuestionCommand { get; }
        public ICommand GoGradingSystemCommand { get; }
        public ICommand CancelCommand { get; }

        private readonly TestService _testService;
        private readonly QuestionService _questionService;
        private readonly OptionService _optionService;

        private QuestionCreateViewModel? _selectedQuestion;
        public QuestionCreateViewModel? SelectedQuestion
        {
            get => _selectedQuestion;
            set
            {
                if (_selectedQuestion != null)
                    _selectedQuestion.IsSelected = false;

                SetProperty(ref _selectedQuestion, value);

                if (value != null)
                    value.IsSelected = true;
            }
        }

        public CuratorCreateTestViewModel(INavigationService navigationService, IDialogService dialogService,
                OptionService optionService,
                QuestionService questionService,
                TestService testService,
                DTestTypeService dTestTypeService,
                TopicService topicService,
                CriteriaService criteriaService,
                CorrespondenceService correspondenceService, ILoggingService logger)
                : base(navigationService, dialogService, logger)
        {
            IsTestFinished = false;
            _optionService = optionService;
            _questionService = questionService;
            _testService = testService;
            _dTestTypeService = dTestTypeService;
            _topicService = topicService;
            _criteriaService = criteriaService;
            _correspondenceService = correspondenceService;

            IsEditMode = false;
            Test = new Test();
            _ = LoadReferenceDataAsync();

            AddQuestionCommand = new RelayCommand(_ => AddNewQuestion());
            AddTextQuestionCommand = new RelayCommand(_ => AddNewTextQuestion());
            AddComplianceQuestionCommand = new RelayCommand(_ => AddNewComplianceQuestion());
            RemoveQuestionCommand = new RelayCommand(question => RemoveQuestion(question));
            SaveTestCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());
            SelectQuestionCommand = new RelayCommand(obj =>
            {
                if (obj is QuestionCreateViewModel question)
                {
                    SelectedQuestion = question;
                    Questions[SelectedQuestion.NewQuestion.NumberActual - 1].IsSelected = true;

                    foreach (var q in Questions)
                    {
                        if (q.NewQuestion.NumberActual != SelectedQuestion.NewQuestion.NumberActual)
                            q.IsSelected = false;
                    }
                }
            });
        }

        public CuratorCreateTestViewModel(INavigationService navigationService,
            IDialogService dialogService,
            OptionService optionService,
            QuestionService questionService,
            TestService testService,
            DTestTypeService dTestTypeService,
            TopicService topicService,
            CriteriaService criteriaService,
            CorrespondenceService correspondenceService,
        Test test, ILoggingService logger)
            : base(navigationService, dialogService, logger)
        {
            _optionService = optionService;
            _questionService = questionService;
            _testService = testService;
            _dTestTypeService = dTestTypeService;
            _topicService = topicService;
            _criteriaService = criteriaService;
            _correspondenceService = correspondenceService;

            IsEditMode = true;

            _originalTest = test;
            _originalQuestions = new List<Question>();
            _originalOptions = new List<Option>();
            _originalCorrespondences = new List<Сorrespondence>();

            Test = CloneTest(test);
            _ = LoadTestForEditAsync(test);

            AddQuestionCommand = new RelayCommand(_ => AddNewQuestion());
            AddTextQuestionCommand = new RelayCommand(_ => AddNewTextQuestion());
            AddComplianceQuestionCommand = new RelayCommand(_ => AddNewComplianceQuestion());
            GoGradingSystemCommand = new RelayCommand(_ => GoGradingSystem());
            RemoveQuestionCommand = new RelayCommand(question => RemoveQuestion(question));
            SaveTestCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());
            SelectQuestionCommand = new RelayCommand(obj =>
            {
                if (obj is QuestionCreateViewModel question)
                {
                    SelectedQuestion = question;
                }
            });

            if (Questions.Count != 0)
                SelectedQuestion = Questions.First();
            else if (SelectedQuestion != null)
                SelectedQuestion.IsQuestionType = 42;
        }

        private Test CloneTest(Test original)
        {
            return new Test
            {
                Id = original.Id,
                Name = original.Name,
                Description = original.Description,
                TimeLimitSecond = original.TimeLimitSecond,
                MaxNumPassing = original.MaxNumPassing,
                TopicId = original.TopicId,
                TestTypeId = original.TestTypeId,
                PenaltyMax = original.PenaltyMax,
                IsRandom = original.IsRandom,
                IsShowNowAnswer = original.IsShowNowAnswer,
                IsArchive = original.IsArchive,
            };
        }
        public List<Question> DeletedQuestions { get; } = new();
        public void RemoveQuestion(object parameter)
        {
            if (parameter is QuestionCreateViewModel questionVM)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить вопрос?",
                                             "Подтверждение удаления",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (questionVM.NewQuestion.Id != 0)
                    {
                        questionVM.NewQuestion.IsArchive = true;
                        DeletedQuestions.Add(questionVM.NewQuestion);
                    }

                    Questions.Remove(questionVM);
                    QuestionNumber--;

                    int index = 1;
                    foreach (var q in Questions)
                    {
                        q.NewQuestion.NumberActual = index++;
                    }
                }
            }
        }

        public void Save()
        {
            if (SelectedTopic == null)
            {
                MessageBox.Show("Выберите тему", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Questions.Any())
            {
                MessageBox.Show("Добавьте хотя бы один вопрос", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (var q in Questions)
            {
                if (string.IsNullOrWhiteSpace(q.Text))
                {
                    MessageBox.Show("Заполните текст всех вопросов", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!q.Options.Any() && q.IsQuestionType == 1)
                {
                    MessageBox.Show($"Добавьте варианты ответа к вопросу", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!q.Options.Any(o => o.IsCorrect == true) && q.IsQuestionType == 1)
                {
                    MessageBox.Show($"Отметьте правильный ответ в вопросе", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                foreach (var opt in q.Options.Where(o => o.IsImage && string.IsNullOrEmpty(o.Value)))
                {
                    MessageBox.Show($"Выберите изображение для варианта ответа", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (IsEditMode)
            {
                UpdateTest();
            }
            else
            {
                CreateTest();
            }
        }

        private async void CreateTest()
        {
            try
            {
                IsLoading = true;
                Test.IsRandom = TestIsRandom;
                Test.IsShowNowAnswer = TestIsShowNowAnswer;

                await _testService.AddAsync(Test, Questions.Count);

                await _criteriaService.AddAsync(new Criteria { TestId = Test.Id, Name = "5 (отлично)", MinPercent = 85, IsActive = true, OrderNumber = 5 });
                await _criteriaService.AddAsync(new Criteria { TestId = Test.Id, Name = "4 (хорошо)", MinPercent = 70, IsActive = true, OrderNumber = 4 });
                await _criteriaService.AddAsync(new Criteria { TestId = Test.Id, Name = "3 (удовлетворительно)", MinPercent = 50, IsActive = true, OrderNumber = 3 });
                await _criteriaService.AddAsync(new Criteria { TestId = Test.Id, Name = "2 (неудовлетворительно)", MinPercent = 0, IsActive = true, OrderNumber = 2 });
                await _criteriaService.AddAsync(new Criteria { TestId = Test.Id, Name = "1 (неявка/незачёт)", MinPercent = 0, IsActive = true, OrderNumber = 1 });

                int qi = 1;
                foreach (var qvm in Questions)
                {
                    qvm.NewQuestion.TestId = Test.Id;
                    qvm.NewQuestion.NumberActual = qi++;
                    qvm.NewQuestion.IsRandom = false;
                    qvm.NewQuestion.Version = 1;

                    var savedQuestion = await _questionService.AddAsync(qvm.NewQuestion, Test, qi - 1);

                    if (qvm.IsQuestionType == 1)
                    {
                        int oi = 1;
                        foreach (var opt in qvm.Options)
                        {
                            opt.NewOption.QuestionId = savedQuestion.Id;
                            opt.NewOption.Number = oi++;
                            await _optionService.AddAsync(opt.NewOption, (int)opt.NewOption.Number);
                        }
                    }
                    else if (qvm.IsQuestionType == 2)
                    {
                        if (!string.IsNullOrWhiteSpace(qvm.CorrectAnswerForTextAnswerType))
                        {
                            var textOption = new Option
                            {
                                QuestionId = savedQuestion.Id,
                                TextAnswer = qvm.CorrectAnswerForTextAnswerType,
                                IsCorrect = true,
                                Number = 1,
                                IsImage = false,
                            };
                            await _optionService.AddAsync(textOption, 1);
                        }
                    }
                    else if (qvm.IsQuestionType == 3)
                    {
                        int oi = 1;
                        var constantOptionsList = new List<Option>();

                        foreach (var opt in qvm.ConstantOptions)
                        {
                            opt.NewOption.QuestionId = savedQuestion.Id;
                            opt.NewOption.Number = oi++;
                            var savedConstant = await _optionService.AddReturnAsync(opt.NewOption, (int)opt.NewOption.Number);
                            constantOptionsList.Add(savedConstant);
                        }

                        var correspondingOptionsList = new List<Option>();
                        foreach (var opt in qvm.Options)
                        {
                            opt.NewOption.QuestionId = savedQuestion.Id;
                            opt.NewOption.Number = oi++;
                            var savedCorresponding = await _optionService.AddReturnAsync(opt.NewOption, (int)opt.NewOption.Number);
                            correspondingOptionsList.Add(savedCorresponding);
                        }

                        var correspondences = new List<(int constantId, int correspondingId)>();
                        for (int i = 0; i < qvm.ConstantOptions.Count && i < qvm.CorrespondingOptions.Count; i++)
                        {
                            var constantOpt = qvm.ConstantOptions[i];
                            var correspondingVM = qvm.CorrespondingOptions[i];

                            if (i < constantOptionsList.Count && correspondingVM.SelectedMatch != null)
                            {
                                var matchedIndex = qvm.Options.IndexOf(correspondingVM.SelectedMatch);
                                if (matchedIndex >= 0 && matchedIndex < correspondingOptionsList.Count)
                                {
                                    correspondences.Add((constantOptionsList[i].Id, correspondingOptionsList[matchedIndex].Id));
                                }
                            }
                        }
                        if (correspondences.Any())
                        {
                            await _correspondenceService.SaveForQuestionAsync(savedQuestion.Id, correspondences);
                        }
                    }
                }

                MessageBox.Show("Тест успешно сохранён!");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка при сохранении теста: {ex.Message}", "Ошибка");
                var innerException = ex.InnerException;
                while (innerException != null)
                {
                    Debug.WriteLine($"Inner Exception: {innerException.Message}");
                    innerException = innerException.InnerException;
                }
                throw;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void UpdateTest()
        {
            try
            {
                IsLoading = true;

                Test.IsArchive = _originalTest.IsArchive;
                Test.IsRandom = TestIsRandom;
                Test.IsShowNowAnswer = TestIsShowNowAnswer;
                Test.PenaltyMax = Questions.Count;
                await _testService.UpdateAsync(Test);

                var dbQuestions = await _questionService.GetQuestiosForCurrentTestAsync(Test.Id);

                foreach (var deleted in DeletedQuestions)
                {
                    await _questionService.UpdateAsync(deleted);
                }

                var maxVersionsByNumber = dbQuestions
                    .Where(q => q.IsArchive != true)
                    .GroupBy(q => q.NumberActual)
                    .ToDictionary(g => g.Key, g => g.Max(q => q.Version));

                int qi = 1;
                foreach (var qvm in Questions)
                {
                    qvm.NewQuestion.TestId = Test.Id;
                    qvm.NewQuestion.NumberActual = qi++;

                    if (qvm.NewQuestion.Id == 0)
                    {
                        int currentNumber = qvm.NewQuestion.NumberActual;
                        int lastVersion = maxVersionsByNumber.ContainsKey(currentNumber)
                            ? maxVersionsByNumber[currentNumber]
                            : 0;

                        qvm.NewQuestion.Version = lastVersion > 0 ? lastVersion + 1 : 1;
                        qvm.NewQuestion.IsArchive = false;

                        var savedQuestion = await _questionService.AddAsync(qvm.NewQuestion, Test, qi - 1);

                        await SaveOptionsForQuestion(qvm, savedQuestion);
                    }
                    else
                    {
                        bool needsNewVersion = await CheckQuestionNeedsNewVersion(qvm, dbQuestions);

                        if (needsNewVersion)
                        {
                            var oldQuestion = dbQuestions.FirstOrDefault(q => q.Id == qvm.NewQuestion.Id);
                            if (oldQuestion != null)
                            {
                                oldQuestion.IsArchive = true;
                                await _questionService.UpdateAsync(oldQuestion);
                            }

                            int currentNumber = qvm.NewQuestion.NumberActual;
                            int lastVersion = maxVersionsByNumber.ContainsKey(currentNumber)
                                ? maxVersionsByNumber[currentNumber]
                                : 0;

                            qvm.NewQuestion.Version = lastVersion + 1;
                            qvm.NewQuestion.Id = 0; 
                            qvm.NewQuestion.IsArchive = false;

                            maxVersionsByNumber[currentNumber] = qvm.NewQuestion.Version;

                            var savedQuestion = await _questionService.AddAsync(qvm.NewQuestion, Test, qi - 1);

                            await SaveOptionsForQuestion(qvm, savedQuestion);
                        }
                        else
                        {
                            await _questionService.UpdateAsync(qvm.NewQuestion);

                            if (qvm.IsQuestionType == 3)
                            {
                                await UpdateCorrespondencesOnly(qvm, qvm.NewQuestion);
                            }
                        }
                    }
                }

                await _logger.LogAsync(
                   whoMade: CurrentUser.Name,
                   whoRole: "CozyTest.Models.Curator",
                   action: LogActionType.Edit,
                   objectType: LogObjectType.Test,
                   objectName: Test.Name
                );

                MessageBox.Show("Тест успешно обновлён!");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка при обновлении теста: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task<bool> CheckQuestionNeedsNewVersion(QuestionCreateViewModel qvm, List<Question> dbQuestions)
        {
            var dbQuestion = dbQuestions.FirstOrDefault(q => q.Id == qvm.NewQuestion.Id);
            if (dbQuestion == null) return true; 

            var dbOptions = await _optionService.GetOptionsByQuestionAsync(dbQuestion.Id);

            int currentOptionsCount = qvm.IsQuestionType == 3
                ? qvm.ConstantOptions.Count + qvm.Options.Count
                : qvm.Options.Count;

            if (currentOptionsCount != dbOptions.Count) return true;

            if (qvm.IsQuestionType == 1 || qvm.IsQuestionType == 2)
            {
                var currentOptions = qvm.Options.OrderBy(o => o.NewOption.Number).ToList();
                var dbOptionsSorted = dbOptions.OrderBy(o => o.Number).ToList();

                for (int i = 0; i < currentOptions.Count; i++)
                {
                    if (i >= dbOptionsSorted.Count) return true;

                    var current = currentOptions[i].NewOption;
                    var db = dbOptionsSorted[i];

                    if (current.TextAnswer != db.TextAnswer ||
                        current.IsCorrect != db.IsCorrect ||
                        current.IsImage != db.IsImage)
                        return true;
                }
            }
            else if (qvm.IsQuestionType == 3)
            {

                var correspondences = await _correspondenceService.GetByQuestionIdAsync(dbQuestion.Id);
                var constantIds = correspondences.Select(c => c.ConstantId).Distinct().ToHashSet();

                var dbConstants = dbOptions.Where(o => constantIds.Contains(o.Id)).ToList();
                var dbCorrespondings = dbOptions.Where(o => !constantIds.Contains(o.Id)).ToList();

                if (qvm.ConstantOptions.Count != dbConstants.Count()) return true;
                if (qvm.Options.Count != dbCorrespondings.Count) return true;

                var currentConstants = qvm.ConstantOptions.OrderBy(o => o.NewOption.Number).ToList();
                var dbConstantsSorted = dbConstants.OrderBy(o => o.Number).ToList();

                for (int i = 0; i < currentConstants.Count; i++)
                {
                    if (i >= dbConstantsSorted.Count()) return true;
                    if (currentConstants[i].NewOption.TextAnswer != dbConstantsSorted[i].TextAnswer)
                        return true;
                }
            }

            return false;
        }

        private async Task SaveOptionsForQuestion(QuestionCreateViewModel qvm, Question savedQuestion)
        {
            if (qvm.IsQuestionType == 3)
            {
                var constantOptionsWithIds = new List<Option>();
                int ci = 1;
                foreach (var opt in qvm.ConstantOptions)
                {
                    opt.NewOption.QuestionId = savedQuestion.Id;
                    opt.NewOption.Number = ci++;
                    var savedConstant = await _optionService.AddReturnAsync(opt.NewOption, (int)opt.NewOption.Number);
                    constantOptionsWithIds.Add(savedConstant);
                    opt.NewOption.Id = savedConstant.Id;
                }

                var correspondingOptionsWithIds = new List<Option>();
                int ri = 1;
                foreach (var opt in qvm.Options)
                {
                    opt.NewOption.QuestionId = savedQuestion.Id;
                    opt.NewOption.Number = ri++;
                    var savedCorresponding = await _optionService.AddReturnAsync(opt.NewOption, (int)opt.NewOption.Number);
                    correspondingOptionsWithIds.Add(savedCorresponding);
                    opt.NewOption.Id = savedCorresponding.Id;
                }

                var correspondences = new List<(int constantId, int correspondingId)>();
                for (int i = 0; i < qvm.ConstantOptions.Count && i < qvm.CorrespondingOptions.Count; i++)
                {
                    var constantOpt = qvm.ConstantOptions[i];
                    var correspondingVM = qvm.CorrespondingOptions[i];

                    int constantId = constantOpt.NewOption.Id;
                    int correspondingId = 0;

                    if (correspondingVM.SelectedMatch != null)
                    {
                        correspondingId = correspondingVM.SelectedMatch.NewOption.Id;
                    }

                    if (constantId > 0 && correspondingId > 0)
                    {
                        correspondences.Add((constantId, correspondingId));
                    }
                }

                if (correspondences.Any())
                {
                    await _correspondenceService.SaveForQuestionAsync(savedQuestion.Id, correspondences);
                }
            }
            else
            {
                int oi = 1;
                foreach (var opt in qvm.Options)
                {
                    opt.NewOption.QuestionId = savedQuestion.Id;
                    opt.NewOption.Number = oi++;
                    await _optionService.AddAsync(opt.NewOption, (int)opt.NewOption.Number);
                }
            }
        }

        private async Task UpdateCorrespondencesOnly(QuestionCreateViewModel qvm, Question savedQuestion)
        {
            await _correspondenceService.RemoveByQuestionIdAsync(savedQuestion.Id);

            var correspondences = new List<(int constantId, int correspondingId)>();

            var dbOptions = await _optionService.GetOptionsByQuestionAsync(savedQuestion.Id);

            for (int i = 0; i < qvm.ConstantOptions.Count && i < qvm.CorrespondingOptions.Count; i++)
            {
                var constantVM = qvm.ConstantOptions[i];
                var correspondingVM = qvm.CorrespondingOptions[i];

                var constantDb = dbOptions.FirstOrDefault(o =>
                    o.Number == constantVM.NewOption.Number &&
                    o.TextAnswer == constantVM.NewOption.TextAnswer);

                if (constantDb == null) continue;

                if (correspondingVM.SelectedMatch != null)
                {
                    var correspondingDb = dbOptions.FirstOrDefault(o =>
                        o.Number == correspondingVM.SelectedMatch.NewOption.Number &&
                        o.TextAnswer == correspondingVM.SelectedMatch.NewOption.TextAnswer);

                    if (correspondingDb != null)
                    {
                        correspondences.Add((constantDb.Id, correspondingDb.Id));
                    }
                }
            }

            if (correspondences.Any())
            {
                await _correspondenceService.SaveForQuestionAsync(savedQuestion.Id, correspondences);
            }
        }

        private void AddNewQuestion()
        {
            var question = new Question { QuestionTypeId = 1, NumberActual = ++QuestionNumber };
            var questionVM = new QuestionCreateViewModel(this, _optionService, question);
            Questions.Add(questionVM);
            SelectedQuestion = questionVM;
            OnPropertyChanged(nameof(HasQuestions));
        }

        private void AddNewTextQuestion()
        {
            var question = new Question { QuestionTypeId = 2, NumberActual = ++QuestionNumber };
            var questionVM = new QuestionCreateViewModel(this, _optionService, question);
            Questions.Add(questionVM);
            SelectedQuestion = questionVM;
            OnPropertyChanged(nameof(HasQuestions));
        }

        private void AddNewComplianceQuestion()
        {
            var question = new Question { QuestionTypeId = 3, NumberActual = ++QuestionNumber };
            var questionVM = new QuestionCreateViewModel(this, _optionService, question);
            Questions.Add(questionVM);
            SelectedQuestion = questionVM;
            OnPropertyChanged(nameof(HasQuestions));
        }

        private async Task LoadTestForEditAsync(Test test)
        {
            try
            {
                IsLoading = true;

                await LoadReferenceDataAsync();

                var fullTest = await _testService.GetTestWithDetailsAsync(test.Id);

                if (fullTest == null)
                {
                    _dialogService.ShowMessage("Не удалось загрузить тест", "Ошибка");
                    return;
                }

                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    _originalQuestions.Clear();
                    _originalOptions.Clear();

                    foreach (var q in fullTest.Questions)
                    {
                        _originalQuestions.Add(CloneQuestion(q));
                        foreach (var opt in q.Options)
                        {
                            _originalOptions.Add(CloneOption(opt));
                        }
                    }

                    foreach (var q in fullTest.Questions)
                    {
                        if (q.QuestionTypeId == 3)
                        {
                            var corrs = await _correspondenceService.GetByQuestionIdAsync(q.Id);
                            _originalCorrespondences.AddRange(corrs.Select(c => new Сorrespondence
                            {
                                ConstantId = c.ConstantId,
                                СorrespondingId = c.СorrespondingId
                            }));
                        }
                    }

                    Test = CloneTest(fullTest);
                    SelectedTopic = Topics.FirstOrDefault(s => s.Id == Test.TopicId);
                    SelectedDTestType = DTestTypes.FirstOrDefault(s => s.Id == Test.TestTypeId);
                    TestMaxNumPassing = Test.MaxNumPassing;
                    TestTimeLimit = Test.TimeLimitSecond;
                    TestIsRandom = Test.IsRandom;
                    TestIsShowNowAnswer = Test.IsShowNowAnswer;

                    Questions.Clear();
                    QuestionNumber = 0;

                    var latestQuestions = fullTest.Questions?
                                        .Where(q => q.IsArchive != true)
                                        .GroupBy(q => q.NumberActual)
                                        .Select(g => g.OrderByDescending(q => q.Version).First())
                                        .OrderBy(q => q.NumberActual)
                                        .ToList() ?? new List<Question>();

                    foreach (var q in latestQuestions)
                    {
                        QuestionNumber++;
                        var qvm = new QuestionCreateViewModel(this, _optionService, CloneQuestion(q));
                        qvm.IsRandom = q.IsRandom;

                        var options = q.Options?.OrderBy(o => o.Number).ToList() ?? new List<Option>();

                        switch (q.QuestionTypeId)
                        {
                            case 1:
                                foreach (var opt in options)
                                {
                                    bool isImage = opt.IsImage ?? false;
                                    var optVM = new OptionCreateViewModel(isImage, qvm, this) { NewOption = CloneOption(opt) };
                                    qvm.Options.Add(optVM);
                                }
                                break;

                            case 2:
                                var textOption = options.FirstOrDefault(o => o.IsCorrect == true);
                                if (textOption != null)
                                {
                                    qvm.CorrectAnswerForTextAnswerType = textOption.TextAnswer;
                                }
                                break;

                            case 3:
                                var correspondences = await _correspondenceService.GetByQuestionIdAsync(q.Id);

                                var constantIds = correspondences.Select(c => c.ConstantId).Distinct().ToList();
                                var constants = options.Where(o => constantIds.Contains(o.Id)).ToList();

                                var correspondingIds = correspondences.Select(c => c.СorrespondingId).Distinct().ToList();
                                var correspondings = options.Where(o => correspondingIds.Contains(o.Id)).ToList();

                                foreach (var opt in constants)
                                {
                                    var ocvm = new OptionCreateViewModel(false, qvm, this)
                                    {
                                        NewOption = CloneOption(opt),
                                        IsConstant = true
                                    };
                                    qvm.ConstantOptions.Add(ocvm);
                                }

                                foreach (var opt in correspondings)
                                {
                                    var ocvm = new OptionCreateViewModel(false, qvm, this)
                                    {
                                        NewOption = CloneOption(opt)
                                    };
                                    qvm.Options.Add(ocvm);
                                }

                                for (int i = 0; i < qvm.ConstantOptions.Count; i++)
                                {
                                    var constantOpt = qvm.ConstantOptions[i];
                                    var correspondingVM = new OptionCreateViewModel(false, qvm, this)
                                    {
                                        IsCorresponding = true,
                                        AvailableMatches = new ObservableCollection<OptionCreateViewModel>(qvm.Options),
                                        NewOption = new Option { TextAnswer = "" }
                                    };

                                    var corr = correspondences.FirstOrDefault(c => c.ConstantId == constantOpt.NewOption.Id);
                                    if (corr != null)
                                    {
                                        var matchedOption = qvm.Options.FirstOrDefault(o => o.NewOption.Id == corr.СorrespondingId);
                                        if (matchedOption != null)
                                        {
                                            correspondingVM.SelectedMatch = matchedOption;
                                        }
                                    }

                                    qvm.CorrespondingOptions.Add(correspondingVM);
                                }
                                break;
                        }

                        Questions.Add(qvm);
                        OnPropertyChanged(nameof(HasQuestions));
                    }

                    if (Questions.Count > 0)
                    {
                        SelectedQuestion = Questions.First();
                    }
                });
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка загрузки теста: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private Question CloneQuestion(Question original)
        {
            return new Question
            {
                Id = original.Id,
                TestId = original.TestId,
                NumberActual = original.NumberActual,
                TestQuest = original.TestQuest,
                PicturePath = original.PicturePath,
                Comments = original.Comments,
                QuestionTypeId = original.QuestionTypeId,
                Version = original.Version, 
                IsRandom = original.IsRandom,
                IsArchive = original.IsArchive
            };
        }

        private Option CloneOption(Option original)
        {
            return new Option
            {
                Id = original.Id,
                QuestionId = original.QuestionId,
                Number = original.Number,
                TextAnswer = original.TextAnswer,
                IsCorrect = original.IsCorrect,
                IsImage = original.IsImage
            };
        }

        private async Task LoadReferenceDataAsync()
        {
            try
            {
                IsLoading = true;

                await _topicService.InitializeAsync();
                await _dTestTypeService.InitializeAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Topics.Clear();
                    foreach (var topic in _topicService.Topics)
                    {
                        Topics.Add(topic);
                    }

                    DTestTypes.Clear();
                    foreach (var type in _dTestTypeService.DTestTypes)
                    {
                        DTestTypes.Add(type);
                    }
                });
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка загрузки справочников: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void GoGradingSystem()
        {
            var g = new GradingSystemViewModel(_navigationService, _dialogService, _criteriaService, Test, _logger);
            _dialogService.ShowWindow<ShellWindow>(g);
        }

        private void Cancel()
        {
            var result = MessageBox.Show("Вы уверены, что хотите отменить изменения? Все несохранённые данные будут потеряны.",
                                         "Подтверждение отмены",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _navigationService.GoBack();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
    }
}