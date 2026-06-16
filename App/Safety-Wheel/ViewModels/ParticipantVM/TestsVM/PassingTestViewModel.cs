using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CreateTestsVM;
using CozyTest.ViewModels.CuratorVM;
using CozyTest.ViewModels.ParticipantVM.TestsVM;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CozyTest.ViewModels.ParticipantVM
{
    public class PassingTestViewModel : BaseViewModel
    {
        public override string WindowTitle => "Прохождение теста";

        private readonly TestService _testService;
        private readonly QuestionService _questionService;
        private readonly OptionService _optionService;
        private readonly AttemptService _attemptService;
        private readonly ParticipantAnswerService _participantAnswerService;
        private readonly CorrespondenceService _correspondenceService;
        private readonly CriteriaService _criteriaService;
        private readonly IServiceProvider _serviceProvider;

        private Test _currentTest;

        public Attempt _attempt;
        public Attempt Attempt
        {
            get => _attempt;
            set
            {
                _attempt = value;
                OnPropertyChanged();
            }
        }

        private DateTime _startTime;

        private ObservableCollection<QuestionPassingViewModel> _questions;
        private QuestionPassingViewModel _selectedQuestion;
        private string _timeLimit;
        private System.Timers.Timer _timer;
        private int _remainingSeconds;

        public object _buttonColor = App.Current.Resources["MainSwamp"];
        public string _buttonText = "Принять ответ";

        public object ButtonColor
        {
            get => _buttonColor;
            set
            {
                _buttonColor = value;
                OnPropertyChanged();
            }
        }
        public string ButtonText
        {
            get => _buttonText;
            set
            {
                _buttonText = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<QuestionPassingViewModel> Questions
        {
            get => _questions;
            set => SetProperty(ref _questions, value);
        }

        public QuestionPassingViewModel SelectedQuestion
        {
            get => _selectedQuestion;
            set
            {
                _selectedQuestion = value;
                OnPropertyChanged();
                if (value != null)
                {
                    LoadQuestionContent(value);
                }
            }
        }

        private bool _isTestFinished;
        public bool IsTestFinished
        {
            get => _isTestFinished;
            set
            {
                _isTestFinished = value;
                OnPropertyChanged();
            }
        }

        public string TimeLimit
        {
            get => _timeLimit;
            set => SetProperty(ref _timeLimit, value);
        }

        public int TotalQuestionsCount => Questions?.Count ?? 0;

        public int AnsweredCount => Questions?.Count(q => q.IsAnswered) ?? 0;

        public int CorrectAnswersCount
        {
            get
            {
                if (Questions == null) return 0;

                int correct = 0;
                foreach (var q in Questions)
                {
                    if (!q.IsAnswered) continue;

                    if (q.Question.QuestionTypeId == 1)
                    {
                        var correctOptionIds = q.Options
                            .Where(o => o.Option.IsCorrect == true)
                            .Select(o => o.Option.Id)
                            .OrderBy(id => id);

                        var selectedOptionIds = q.Options
                            .Where(o => o.IsSelected)
                            .Select(o => o.Option.Id)
                            .OrderBy(id => id);

                        if (correctOptionIds.SequenceEqual(selectedOptionIds))
                            correct++;
                    }
                    else if (q.Question.QuestionTypeId == 2)
                    {
                        var correctOption = q.Question.Options.FirstOrDefault(o => o.IsCorrect == true);
                        if (correctOption != null)
                        {
                            var isCorrect = string.Equals(q.TextAnswer?.Trim(),
                                correctOption.TextAnswer?.Trim(),
                                StringComparison.OrdinalIgnoreCase);
                            if (isCorrect) correct++;
                        }
                    }
                    else if (q.Question.QuestionTypeId == 3)
                    {
                        bool allCorrect = true;
                        foreach (var pair in q.MatchingPairs)
                        {
                            if (pair.SelectedMatch == null)
                            {
                                allCorrect = false;
                                break;
                            }
                        }
                        if (allCorrect && q.MatchingPairs.All(p => p.SelectedMatch != null))
                            correct++;
                    }
                }
                return correct;
            }
        }

        public double ScorePercent => TotalQuestionsCount > 0
            ? (double)CorrectAnswersCount / TotalQuestionsCount * 100
            : 0;

        public bool IsScoreGood => ScorePercent >= 75;
        public bool IsScoreBad => ScorePercent < 40;

        public ICommand SelectQuestionCommand { get; }
        public ICommand SubmitAnswerCommand { get; }

        public PassingTestViewModel(
            INavigationService navigationService,
            IDialogService dialogService,
            IServiceProvider serviceProvider,
            TestService testService,
            QuestionService questionService,
            OptionService optionService,
            AttemptService attemptService,
            ParticipantAnswerService participantAnswerService,
            CorrespondenceService correspondenceService,
            CriteriaService criteriaService,
            Test test, ILoggingService logger) : base(navigationService, dialogService, logger)
        {
            _serviceProvider = serviceProvider;
            IsTestFinished =false;
            _testService = testService;
            _questionService = questionService;
            _optionService = optionService;
            _attemptService = attemptService;
            _participantAnswerService = participantAnswerService;
            _correspondenceService = correspondenceService;
            _criteriaService = criteriaService;

            _currentTest = test;

            SelectQuestionCommand = new RelayCommand(obj =>
            {
                if (obj is QuestionPassingViewModel question)
                {
                    SelectedQuestion = question;
                    Questions[SelectedQuestion.Question.NumberActual - 1].IsSelected = true;

                    foreach(var q in Questions)
                    {
                        if (q.Question.NumberActual != SelectedQuestion.Question.NumberActual)
                        q.IsSelected = false;
                    }
                }
            });

            SubmitAnswerCommand = new RelayCommand(_ => SubmitCurrentAnswer());

            Task.Run(async () => await LoadTestAsync());

        }

        private async Task LoadTestAsync()
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() => IsLoading = true);

                _currentTest = await _testService.GetTestWithDetailsAsync(_currentTest.Id);

                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    if (_currentTest == null)
                    {
                        _dialogService.ShowMessage("Тест не найден", "Ошибка");
                        return;
                    }

                    await CreateAttempt();

                    var latestQuestions = _currentTest.Questions
                        .Where(p => p.IsArchive != true)
                        .GroupBy(q => q.NumberActual)
                        .Select(g => g.OrderByDescending(q => q.Version).First())
                        .ToList();

                    var sortedQuestions = latestQuestions.OrderBy(q => q.NumberActual).ToList();

                    if (_currentTest.IsRandom == true)
                    {
                        sortedQuestions = sortedQuestions.OrderBy(x => Guid.NewGuid()).ToList();
                    }

                    Questions = new ObservableCollection<QuestionPassingViewModel>();

                    foreach (var question in sortedQuestions)
                    {
                        var qvm = new QuestionPassingViewModel(question, _optionService, _correspondenceService);
                        await qvm.LoadOptionsAsync();

                        if (question.IsRandom == true && question.QuestionTypeId == 1)
                        {
                            qvm.Options = new ObservableCollection<OptionPassingViewModel>(qvm.Options.OrderBy(x => Guid.NewGuid()));
                        }

                        Questions.Add(qvm);
                    }

                    if (Questions.Any())
                    {
                        SelectedQuestion = Questions.First();
                        Questions[SelectedQuestion.Question.NumberActual - 1].IsSelected = true;
                    }

                    if (_currentTest.TimeLimitSecond > 0)
                    {
                        StartTimer();
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    _dialogService.ShowMessage($"Ошибка загрузки теста: {ex.Message}", "Ошибка"));
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() => IsLoading = false);

                await _logger.LogAsync(
                    whoMade: CurrentUser.Name,
                    whoRole: CurrentUser.ClassUser.ToString(),
                    action: LogActionType.Edit,
                    objectType: LogObjectType.Test,
                    objectName: _currentTest.Name,
                    details: "начал"
                );
            }
        }

        private async Task CreateAttempt()
        {
            int testVersion = _currentTest.Questions.Any()
                ? _currentTest.Questions.Max(q => q.Version)
                : 1;

            var attempt = new Attempt
            {
                ParticipantId = CurrentUser.Id,
                TestId = _currentTest.Id,
                CountQuestions = _currentTest.Questions.Where(p=>p.IsArchive != true).Count(),
                StartedAt = DateTime.Now,
                Status = "В процессе",
                Score = 0,
            };
            await _attemptService.AddAsync(attempt);
            var lastAttempt = await _attemptService.GetLastByTypeAsync(CurrentUser.Id, _currentTest.Id);
            Attempt = lastAttempt;
            _startTime = DateTime.Now;
        }

        private void LoadQuestionContent(QuestionPassingViewModel qvm)
        {
            qvm.LoadContent();
            OnPropertyChanged(nameof(SelectedQuestion));
        }

        bool isFinish = false;
        private async void SubmitCurrentAnswer()
        {
            if (IsTestFinished == true)
            {
                var viewModel = ActivatorUtilities.CreateInstance<PartAllTestViewModel>(_serviceProvider);
                _navigationService.NavigateTo(viewModel);

                return;
            }
                    
            if (SelectedQuestion == null) return;

            if (isFinish == true)
            {
                await FinishTestAsync();
                return;
            }
            else if (SelectedQuestion.IsAnswered)
            {
                MessageBox.Show("Вы уже ответили на этот вопрос", "Внимание");
                return;
            }

            bool isValid = ValidateAnswer(SelectedQuestion);
            if (!isValid) return;

            await SaveAnswerAsync(SelectedQuestion);

            SelectedQuestion.IsAnswered = true;

            int nextIndex = Questions.IndexOf(SelectedQuestion) + 1;
            if (nextIndex < Questions.Count)
            {
                SelectedQuestion.IsSelected = false;
                SelectedQuestion = Questions[nextIndex];
                SelectedQuestion.IsSelected = true;
            }
            else
            {
                var msg = MessageBox.Show("Перейти к завершению теста?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (msg == MessageBoxResult.No) return;
                else await FinishTestAsync();
            }

            if (Questions.FirstOrDefault(p => p.IsAnswered == false) == null)
            {
                isFinish = true;
                ButtonColor = App.Current.Resources["HoverGreen"];
                ButtonText = "Завершить прохождение";
            }
        }

        private bool ValidateAnswer(QuestionPassingViewModel qvm)
        {
            if (qvm.Question.QuestionTypeId == 1)
            {
                if (!qvm.Options.Any(o => o.IsSelected))
                {
                    MessageBox.Show("Выберите хотя бы один вариант ответа", "Внимание");
                    return false;
                }
            }
            else if (qvm.Question.QuestionTypeId == 2)
            {
                if (string.IsNullOrWhiteSpace(qvm.TextAnswer))
                {
                    MessageBox.Show("Введите текстовый ответ", "Внимание");
                    return false;
                }
            }
            else if (qvm.Question.QuestionTypeId == 3)
            {
                if (qvm.MatchingPairs.Any(p => p.SelectedMatch == null))
                {
                    MessageBox.Show("Установите все соответствия", "Внимание");
                    return false;
                }
            }

            return true;
        }

        private async Task SaveAnswerAsync(QuestionPassingViewModel qvm)
        {
            try
            {
                var actualOptions = qvm.Question.Options.ToList();

                if (qvm.Question.QuestionTypeId == 1)
                {
                    var correctOptionIds = actualOptions
                        .Where(o => o.IsCorrect == true)
                        .Select(o => o.Id)
                        .ToHashSet();

                    var selectedOptionIds = qvm.Options
                        .Where(o => o.IsSelected)
                        .Select(o => o.Option.Id)
                        .ToHashSet();

                    bool allCorrectSelected = correctOptionIds.SetEquals(selectedOptionIds);
                    if (allCorrectSelected == false) qvm.IsCorrect = false;
                    else qvm.IsCorrect = true;

                    foreach (var opt in qvm.Options.Where(o => o.IsSelected))
                    {
                        var answer = new ParticipantAnswer
                        {
                            AttemptId = Attempt.Id,
                            QuestionId = qvm.Question.Id,
                            OptionId = opt.Option.Id,
                            IsCorrect = allCorrectSelected,
                            AnsweredAt = DateTime.Now
                        };
                        await _participantAnswerService.AddAsync(answer);
                    }
                }
                else if (qvm.Question.QuestionTypeId == 2)
                {
                    var correctOption = actualOptions.FirstOrDefault(o => o.IsCorrect == true);

                    var isCorrect = string.Equals(qvm.TextAnswer?.Trim(),
                        correctOption?.TextAnswer?.Trim(),
                        StringComparison.OrdinalIgnoreCase);

                    if (isCorrect == false) qvm.IsCorrect = false;
                    else qvm.IsCorrect = true;

                    var answer = new ParticipantAnswer
                    {
                        AttemptId = Attempt.Id,
                        QuestionId = qvm.Question.Id,
                        OptionId = correctOption?.Id ?? 0,
                        TextAnswer = qvm.TextAnswer,
                        IsCorrect = isCorrect,
                        AnsweredAt = DateTime.Now
                    };
                    await _participantAnswerService.AddAsync(answer);
                }
                else if (qvm.Question.QuestionTypeId == 3)
                {
                    await _participantAnswerService.RemoveByAttemptAndQuestionAsync(Attempt.Id, qvm.Question.Id);
                    int correctCount = 0;
                    foreach (var pair in qvm.MatchingPairs)
                    {
                        if (pair.SelectedMatch != null)
                        {
                            bool isPairCorrect = await _correspondenceService.IsCorrectPairAsync(
                                pair.ConstantOption.Option.Id,
                                pair.SelectedMatch.Option.Id);

                            if (isPairCorrect == true) correctCount++;

                            var answer = new ParticipantAnswer
                            {
                                AttemptId = Attempt.Id,
                                QuestionId = qvm.Question.Id,
                                ConstantOptionId = pair.ConstantOption.Option.Id,
                                OptionId = pair.SelectedMatch.Option.Id,
                                IsCorrect = isPairCorrect,
                                AnsweredAt = DateTime.Now
                            };
                            await _participantAnswerService.AddAsync(answer);
                        }
                    }

                    if (correctCount != qvm.MatchingPairs.Count) qvm.IsCorrect = false;
                    else qvm.IsCorrect = true;
                }

                if (_currentTest.IsShowNowAnswer == true) qvm.CanShow = true;
                else qvm.CanShow = false;
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка сохранения ответа: {ex.Message}", "Ошибка");
            }
        }

        public async Task EmergencyFinishTestAsync()
        {
            IsTestFinished = true;

            var score = await CalculateScore();

            string gradeName = await CalculateGradeAsync(score);
            _timer?.Stop();
            Attempt.Score = score;
            Attempt.FinishedAt = DateTime.Now;
            Attempt.Status = "Завершен(Принудительный выход)";
            Attempt.MarkLvl = CurrentMarkLvl;
            Attempt.AttemptNumber = await _attemptService.GetLastNumPlusOne(_currentTest.Id, CurrentUser.Id);
            await _attemptService.UpdateAsync(Attempt);

            await _logger.LogAsync(
                   whoMade: CurrentUser.Name,
                   whoRole: CurrentUser.ClassUser.ToString(),
                   action: LogActionType.Edit,
                   objectType: LogObjectType.Test,
                   objectName: _currentTest.Name,
                   details: "закончил(Принудительный выход)"
               );
        }
            
        public async Task FinishTestAsync()
        {

            IsTestFinished = true;

            if (SelectedQuestion != null && !SelectedQuestion.IsAnswered)
            {
                await SaveEmptyAnswerAsync(SelectedQuestion);
                SelectedQuestion.IsCorrect = false;
            }

            var ls = Questions.Where(q => q.IsAnswered == false);
            foreach (var q in ls)
            {
                await SaveEmptyAnswerAsync(q);
                q.IsCorrect = false;
            }

            var answers = await _participantAnswerService.GetAllAsync(Attempt.Id);

            if (!answers.Any())
            {
                _dialogService.ShowMessage("Внимание: ответы не были сохранены!", "Ошибка");
            }

            var score = await CalculateScore();

            string gradeName = await CalculateGradeAsync(score);
            _timer?.Stop();
            Attempt.Score = score;
            Attempt.FinishedAt = DateTime.Now;
            Attempt.Status = "Завершен";
            Attempt.MarkLvl = CurrentMarkLvl;
            Attempt.AttemptNumber = await _attemptService.GetLastNumPlusOne(_currentTest.Id, CurrentUser.Id);
            await _attemptService.UpdateAsync(Attempt);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var message = string.IsNullOrEmpty(gradeName)
                    ? $"Тест завершен!\nРезультат: {score} из {TotalQuestionsCount}\nПравильных: {CorrectAnswersCount} ({ScorePercent:F1}%)"
                    : $"Тест завершен!\nРезультат: {score} из {TotalQuestionsCount}\nОценка: {gradeName}\nПравильных: {CorrectAnswersCount} ({ScorePercent:F1}%)";

                MessageBox.Show(message, "Завершение", MessageBoxButton.OK, MessageBoxImage.Information);
            });

            await _logger.LogAsync(
                   whoMade: CurrentUser.Name,
                   whoRole: CurrentUser.ClassUser.ToString(),
                   action: LogActionType.Edit,
                   objectType: LogObjectType.Test,
                   objectName: _currentTest.Name,
                   details: "закончил"
               );


            ButtonColor = App.Current.Resources["Gold"];
            ButtonText = "Выйти из теста";
        }
        int CurrentMarkLvl = 0;
        private async Task SaveEmptyAnswerAsync(QuestionPassingViewModel qvm)
        {
            var answer = new ParticipantAnswer
            {
                AttemptId = Attempt.Id,
                QuestionId = qvm.Question.Id,
                OptionId = 0,
                IsCorrect = false,
                AnsweredAt = DateTime.Now
            };
            await _participantAnswerService.AddAsync(answer);
        }
        private async Task<string> CalculateGradeAsync(int score)
        {
            if (_currentTest == null || TotalQuestionsCount == 0)
                return string.Empty;

            double percent = (double)score / TotalQuestionsCount * 100;
            await _criteriaService.GetAllByTestAsync(_currentTest.Id);

            var criteria = _criteriaService.Criteria;

            if (criteria == null || !criteria.Any())
                return string.Empty; 

            var activeCriteria = criteria
                .Where(c => c.IsActive == true)
                .OrderByDescending(c => c.MinPercent)
                .ToList();


           

            if (!activeCriteria.Any())
                return string.Empty;

            var matchedCriterion = activeCriteria
                .FirstOrDefault(c => percent >= c.MinPercent);

            CurrentMarkLvl = matchedCriterion.OrderNumber;
            return matchedCriterion?.Name ?? activeCriteria.Last().Name; 
        }

        private async Task<int> CalculateScore()
        {
            var answers = await _participantAnswerService.GetAllAsync(Attempt.Id);
            int score = 0;

            var answersByQuestion = answers
                .GroupBy(a => a.QuestionId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var question in Questions)
            {
                if (answersByQuestion.TryGetValue(question.Question.Id, out var questionAnswers))
                {
                    if (question.Question.QuestionTypeId == 1)
                    {
                        var correctOptionIds = question.Question.Options
                            .Where(o => o.IsCorrect == true)
                            .Select(o => o.Id)
                            .ToHashSet();

                        var answeredOptionIds = questionAnswers.Select(a => a.OptionId).ToHashSet();

                        if (correctOptionIds.SetEquals(answeredOptionIds))
                            score++;
                    }
                    else if (question.Question.QuestionTypeId == 2)
                    {
                        if (questionAnswers.Any() && questionAnswers.First().IsCorrect == true)
                            score++;
                    }
                    else if (question.Question.QuestionTypeId == 3)
                    {
                        if (questionAnswers.Any() && questionAnswers.All(a => a.IsCorrect == true))
                            score++;
                    }
                }
            }

            return score;
        }

        private void StartTimer()
        {
            _remainingSeconds = _currentTest.TimeLimitSecond;
            UpdateTimeDisplay();

            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (s, e) =>
            {
                if (_remainingSeconds > 0)
                {
                    _remainingSeconds--;
                    Application.Current.Dispatcher.Invoke(() => UpdateTimeDisplay());
                }
                else
                {
                    _timer.Stop();
                    Application.Current.Dispatcher.Invoke(() => FinishTestAsync());
                }
            };
            _timer.Start();
        }

        private void UpdateTimeDisplay()
        {
            var timeSpan = TimeSpan.FromSeconds(_remainingSeconds);
            TimeLimit = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
    }
}