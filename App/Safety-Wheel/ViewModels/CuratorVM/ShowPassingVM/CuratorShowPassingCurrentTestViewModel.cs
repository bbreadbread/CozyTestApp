using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.ParticipantVM.TestsVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CozyTest.ViewModels.CuratorVM.ShowPassingVM
{
    public class CuratorShowPassingCurrentTestViewModel : BaseViewModel
    {
        public override string WindowTitle => "Просмотр попытки";

        private readonly TestService _testService;
        private readonly QuestionService _questionService;
        private readonly OptionService _optionService;
        private readonly AttemptService _attemptService;
        private readonly ParticipantAnswerService _participantAnswerService;
        private readonly CorrespondenceService _correspondenceService;

        private Test _currentTest;
        private Attempt _attempt;

        private ObservableCollection<QuestionPassingViewModel> _questions;
        private QuestionPassingViewModel _selectedQuestion;
        private string _timeLimit;
        private string _Result;

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

        private bool _isTestFinished = false;
        public bool IsTestFinished
        {
            get => _isTestFinished;
            set => SetProperty(ref _isTestFinished, value);
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
                        if (q.MatchingPairs.All(p => p.SelectedMatch != null))
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
                            if (allCorrect) correct++;
                        }
                    }
                }
                return correct;
            }
        }

        public double ScorePercent => TotalQuestionsCount > 0
            ? (double)CorrectAnswersCount / TotalQuestionsCount * 100
            : 0;

        public string ScorePercentText => $"{ScorePercent:F1}%";

        public string AnsweredText => $"{AnsweredCount} / {TotalQuestionsCount}";

        public string Result
        {
            get => _Result;
            set => SetProperty(ref _Result, value);
        }

        public Attempt Attempt => _attempt;
        public string ParticipantName => _attempt?.Participant?.Name ?? "Неизвестно";
        public DateTime StartedAt => _attempt?.StartedAt ?? DateTime.MinValue;
        public DateTime? FinishedAt => _attempt?.FinishedAt;
        public int Score => _attempt?.Score ?? 0;
        public string Status => _attempt?.Status ?? "";

        public ICommand SelectQuestionCommand { get; }
        public ICommand GoBackCommand { get; }

        public CuratorShowPassingCurrentTestViewModel(
            INavigationService navigationService,
            IDialogService dialogService,
            TestService testService,
            QuestionService questionService,
            OptionService optionService,
            AttemptService attemptService,
            ParticipantAnswerService participantAnswerService,
            CorrespondenceService correspondenceService,
            CuratorShowPassingTestsViewModel viewModel, ILoggingService logger) : base(navigationService, dialogService, logger)
        {
            _testService = testService;
            _questionService = questionService;
            _optionService = optionService;
            _attemptService = attemptService;
            _participantAnswerService = participantAnswerService;
            _correspondenceService = correspondenceService;

            _attempt = _attemptService.GetById(viewModel.SelectedAttempt.Id);
            if (_attempt == null)
            {
                dialogService.ShowMessage("Попытка не выбрана", "Ошибка");
                navigationService.GoBack();
                return;
            }

            SelectQuestionCommand = new RelayCommand(obj =>
            {
                if (obj is QuestionPassingViewModel question)
                {
                    SelectedQuestion = question;
                    Questions[SelectedQuestion.Question.NumberNow - 1].IsSelected = true;

                    foreach (var q in Questions)
                    {
                        if (q.Question.NumberNow != SelectedQuestion.Question.NumberNow)
                            q.IsSelected = false;
                    }
                }
            });

            GoBackCommand = new RelayCommand(_ => navigationService.GoBack());

            LoadAttemptAsync();
        }

        private async void LoadAttemptAsync()
        {
            try
            {
                IsLoading = true;
                IsTestFinished = true;

                _currentTest = await _testService.GetTestWithDetailsAsync((int)_attempt.TestId);

                var participantAnswers = await _participantAnswerService.GetAllAsync(_attempt.Id);

                if (_currentTest == null)
                {
                    _dialogService.ShowMessage("Тест не найден", "Ошибка");
                    return;
                }

                if (_attempt.FinishedAt.HasValue && _attempt.StartedAt != default)
                {
                    var duration = _attempt.FinishedAt.Value - _attempt.StartedAt;
                    TimeLimit = $"{duration}";
                }
                else
                {
                    TimeLimit = "--:--";
                }

                var questionsForView = new List<Question>();

                List<int> answeredQuestionIds = participantAnswers
                    .Select(a => a.QuestionId)
                    .Distinct()
                    .ToList();

                foreach (var questionId in answeredQuestionIds)
                {
                    var question = _currentTest.Questions
                        .FirstOrDefault(q => q.Id == questionId);

                    if (question != null && (question.TimeCreate == null || question.TimeCreate <= _attempt.StartedAt))
                    {
                        questionsForView.Add(question);
                    }
                }

                var sortedQuestions = questionsForView.OrderBy(q => q.NumberActual).ToList();

                Questions = new ObservableCollection<QuestionPassingViewModel>();

                int i = 0;
                bool sel = false;
                foreach (var question in sortedQuestions)
                {
                    var qvm = new QuestionPassingViewModel(question, _optionService, _correspondenceService);
                    if (sel == false)
                    {
                        qvm.IsSelected = true;
                        sel = true;
                    }
                    await qvm.LoadOptionsAsync();
                    qvm.Question.NumberNow = ++i;
                    ApplyParticipantAnswers(qvm, participantAnswers);
                    Questions.Add(qvm);
                }

                SelectedQuestion = Questions.First();

                Result = $"{CorrectAnswersCount} / {TotalQuestionsCount}";
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка загрузки попытки: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyParticipantAnswers(QuestionPassingViewModel qvm, List<ParticipantAnswer> answers)
        {
            var questionAnswers = answers
                .Where(a => a.QuestionId == qvm.Question.Id)
                .ToList();

            if (!questionAnswers.Any())
            {
                qvm.IsAnswered = false;
                qvm.IsCorrect = false;
                return;
            }

            var cur = questionAnswers.FirstOrDefault(p => p.QuestionId == qvm.Question.Id && p.OptionId == 0);
            if (cur != null)
            {
                qvm.IsAnswered = false;
                qvm.IsCorrect = false;
            }
            else qvm.IsAnswered = true;

            

            var actualOptions = qvm.Question.Options.ToList();

            if (qvm.Question.QuestionTypeId == 1)
            {
                var correctOptionIds = actualOptions
                    .Where(o => o.IsCorrect == true)
                    .Select(o => o.Id)
                    .ToHashSet();

                var selectedOptionIds = questionAnswers
                    .Select(a => a.OptionId)
                    .ToHashSet();

                bool allCorrectSelected = correctOptionIds.SetEquals(selectedOptionIds);
                qvm.IsCorrect = allCorrectSelected;

                foreach (var opt in qvm.Options)
                {
                    opt.IsSelected = selectedOptionIds.Contains(opt.Option.Id);
                }
            }
            else if (qvm.Question.QuestionTypeId == 2)
            {
                var correctOption = actualOptions.FirstOrDefault(o => o.IsCorrect == true);
                var answer = questionAnswers.FirstOrDefault();

                if (answer != null)
                {
                    qvm.TextAnswer = answer.TextAnswer ?? "";

                    bool isCorrect = string.Equals(qvm.TextAnswer.Trim(),
                        correctOption?.TextAnswer?.Trim(),
                        StringComparison.OrdinalIgnoreCase);
                    qvm.IsCorrect = isCorrect;
                }
            }
            else if (qvm.Question.QuestionTypeId == 3)
            {
                foreach (var pair in qvm.MatchingPairs)
                {
                    var matchAnswer = questionAnswers.FirstOrDefault(a =>
                        a.ConstantOptionId == pair.ConstantOption.Option.Id);

                    if (matchAnswer != null)
                    {
                        pair.SelectedMatch = pair.AvailableMatches
                            .FirstOrDefault(m => m.Option.Id == matchAnswer.OptionId);
                    }
                }

                bool allCorrect = questionAnswers.All(a => a.IsCorrect == true);
                qvm.IsCorrect = allCorrect;
            }
        }
        private void LoadQuestionContent(QuestionPassingViewModel qvm)
        {
            qvm.LoadContent();
            OnPropertyChanged(nameof(SelectedQuestion));
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
    }
}