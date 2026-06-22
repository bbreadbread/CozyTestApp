using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CuratorVM;
using Microsoft.Extensions.Logging;
using ScottPlot.WPF;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CozyTest.ViewModels.CuratorVM.StatisticsVM
{
    public class StatisticsCurrentTestViewModel : BaseViewModel
    {
        private readonly AttemptService _attemptService;
        private readonly ParticipantAnswerService _participantAnswerService;
        private readonly CorrespondenceService _correspondenceService;

        public Test CurrentTest { get; }

        private int _totalResponses;
        public int TotalResponses
        {
            get => _totalResponses;
            set { _totalResponses = value; OnPropertyChanged(); }
        }

        private double _averageScore;
        public double AverageScore
        {
            get => _averageScore;
            set { _averageScore = value; OnPropertyChanged(); }
        }

        private double _medianScore;
        public double MedianScore
        {
            get => _medianScore;
            set { _medianScore = value; OnPropertyChanged(); }
        }

        private double _minScore;
        public double MinScore
        {
            get => _minScore;
            set { _minScore = value; OnPropertyChanged(); }
        }

        private double _maxScore;
        public double MaxScore
        {
            get => _maxScore;
            set { _maxScore = value; OnPropertyChanged(); }
        }
        private int _countQuest;
        public int CountQuest
        {
            get => _countQuest;
            set { _countQuest = value; OnPropertyChanged(); }
        }

        private double[] _scoreDistributionData;
        public double[] ScoreDistributionData
        {
            get => _scoreDistributionData;
            set { _scoreDistributionData = value; OnPropertyChanged(); }
        }

        private string[] _scoreDistributionLabels;
        public string[] ScoreDistributionLabels
        {
            get => _scoreDistributionLabels;
            set { _scoreDistributionLabels = value; OnPropertyChanged(); }
        }

        private ObservableCollection<QuestionSummary> _QuestionsWrong = new();
        public ObservableCollection<QuestionSummary> QuestionsWrong
        {
            get => _QuestionsWrong;
            set { _QuestionsWrong = value; OnPropertyChanged(); }
        }

        private ObservableCollection<QuestionDetailStatistics> _questionDetails = new();
        public ObservableCollection<QuestionDetailStatistics> QuestionDetails
        {
            get => _questionDetails;
            set { _questionDetails = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Participant> _participants = new();
        public ObservableCollection<Participant> Participants
        {
            get => _participants;
            set { _participants = value; OnPropertyChanged(); }
        }

        private Participant _selectedParticipant;
        public Participant SelectedParticipant
        {
            get => _selectedParticipant;
            set
            {
                _selectedParticipant = value;
                OnPropertyChanged();
                _ = LoadTestStatisticsAsync();
            }
        }
        public ICommand RefreshCommand { get; }
        public ICommand ClearFilterCommand { get; }

        public StatisticsCurrentTestViewModel(
            INavigationService navigationService,
            IDialogService dialogService,
            Test currentTest,
            AttemptService attemptService,
            CorrespondenceService correspondenceService,
            ParticipantAnswerService participantAnswerService, ILoggingService logger)
            : base(navigationService, dialogService, logger)
        {
            CurrentTest = currentTest;
            _attemptService = attemptService;
            _participantAnswerService = participantAnswerService;
            _correspondenceService = correspondenceService;
            RefreshCommand = new RelayCommand(async _ => await LoadTestStatisticsAsync());
            ClearFilterCommand = new RelayCommand(_ => SelectedParticipant = null);

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var attempts = await _attemptService.GetAttemptsByTestAsync(CurrentTest.Id);
            var participantIds = attempts
                .Where(a => a.ParticipantId.HasValue)
                .Select(a => a.ParticipantId.Value)
                .Distinct()
                .ToList();

            var participants = attempts
                .Where(a => a.Participant != null)
                .Select(a => a.Participant)
                .DistinctBy(p => p.Id)
                .OrderBy(p => p.Name)
                .ToList();

            Participants = new ObservableCollection<Participant>(participants);

            await LoadTestStatisticsAsync();
        }

        private async Task LoadTestStatisticsAsync()
        {
            try
            {
                var attempts = await _attemptService.GetAttemptsByTestAsync(CurrentTest.Id);

                var filteredAttempts = SelectedParticipant == null
                    ? attempts.Where(a => a.StartedAt.HasValue && a.FinishedAt.HasValue).ToList()
                    : attempts.Where(a => a.ParticipantId == SelectedParticipant.Id
                                        && a.StartedAt.HasValue && a.FinishedAt.HasValue).ToList();

                TotalResponses = filteredAttempts.Count;

                if (filteredAttempts.Count == 0)
                {
                    CountQuest = 0;
                    AverageScore = 0;
                    MedianScore = 0;
                    MinScore = 0;
                    MaxScore = 0;
                    ScoreDistributionData = Array.Empty<double>();
                    ScoreDistributionLabels = Array.Empty<string>();
                    QuestionsWrong = new ObservableCollection<QuestionSummary>();
                    QuestionDetails = new ObservableCollection<QuestionDetailStatistics>();
                    return;
                }

                var scores = filteredAttempts
                    .Where(a => a.Score.HasValue)
                    .Select(a => a.Score.Value)
                    .OrderBy(s => s)
                    .ToList();

                if (!scores.Any())
                {
                    CountQuest = (int)(filteredAttempts.FirstOrDefault()?.CountQuestions ?? 0);
                    AverageScore = 0;
                    MedianScore = 0;
                    MinScore = 0;
                    MaxScore = 0;
                    ScoreDistributionData = Array.Empty<double>();
                    ScoreDistributionLabels = Array.Empty<string>();
                    QuestionsWrong = new ObservableCollection<QuestionSummary>();
                    QuestionDetails = new ObservableCollection<QuestionDetailStatistics>();
                    return;
                }

                CountQuest = (int)(filteredAttempts.FirstOrDefault()?.CountQuestions ?? 0);
                AverageScore = Math.Round(scores.Average(), 1);
                MinScore = scores.Min();
                MaxScore = scores.Max();

                int count = scores.Count;
                MedianScore = count % 2 == 0
                    ? (scores[count / 2 - 1] + scores[count / 2]) / 2.0
                    : scores[count / 2];

                int maxQuestions = (int)(filteredAttempts.FirstOrDefault()?.CountQuestions ?? 0);
                var distribution = new double[maxQuestions + 1];
                var labels = Enumerable.Range(0, maxQuestions + 1).Select(i => i.ToString()).ToArray();

                foreach (var score in scores)
                {
                    int index = (int)Math.Round((double)score);
                    if (index >= 0 && index < distribution.Length)
                        distribution[index]++;
                }

                ScoreDistributionData = distribution;
                ScoreDistributionLabels = labels;

                var wrongs = new ObservableCollection<QuestionSummary>();
                var details = new ObservableCollection<QuestionDetailStatistics>();

                foreach (var question in CurrentTest.Questions.OrderBy(q => q.NumberActual))
                {
                    var answers = _participantAnswerService.GetAnswersByQuestion(question.Id);

                    var filteredAnswers = SelectedParticipant == null
                        ? answers.ToList()
                        : answers.Where(a => a.Attempt.ParticipantId == SelectedParticipant.Id).ToList();

                    if (!filteredAnswers.Any()) continue;

                    var totalAnswers = filteredAnswers.Count;
                    var correctCount = filteredAnswers.Count(a => a.IsCorrect == true);
                    var incorrectCount = totalAnswers - correctCount;
                    var correctPercent = totalAnswers > 0 ? (double)correctCount / totalAnswers * 100 : 0;
                    var incorrectPercent = totalAnswers > 0 ? (double)incorrectCount / totalAnswers * 100 : 0;

                    if (incorrectPercent > 50)
                    {
                        wrongs.Add(new QuestionSummary
                        {
                            QuestionText = question.TestQuest,
                            CorrectCount = correctCount,
                            TotalCount = totalAnswers
                        });
                    }

                    var optionStats = new ObservableCollection<OptionDetail>();
                    var pairStats = new ObservableCollection<PairDetail>();

                    if (question.QuestionTypeId == 3)
                    {
                        var matchingAnswers = filteredAnswers
                            .Where(a => a.ConstantOptionId != 0 && a.OptionId != 0)
                            .ToList();

                        if (matchingAnswers.Any())
                        {
                            var correspondences = await _correspondenceService.GetByQuestionIdAsync(question.Id);
                            var correctPairs = correspondences.ToDictionary(
                                c => c.ConstantId,
                                c => c.СorrespondingId);

                            var allOptions = question.Options.ToDictionary(o => o.Id, o => o.TextAnswer);
                            var constantOptions = question.Options
                                .Where(o => correctPairs.Keys.Contains(o.Id))
                                .ToDictionary(o => o.Id, o => o.TextAnswer);

                            var pairGroups = matchingAnswers
                                .GroupBy(a => new { a.ConstantOptionId, a.OptionId })
                                .Select(g => new
                                {
                                    ConstantId = g.Key.ConstantOptionId,
                                    SelectedId = g.Key.OptionId,
                                    Count = g.Count()
                                })
                                .ToList();

                            foreach (var constantId in correctPairs.Keys)
                            {
                                var constantText = allOptions.GetValueOrDefault(constantId, "???");
                                var correctCorrespondingId = correctPairs[constantId];
                                var correctText = allOptions.GetValueOrDefault(correctCorrespondingId, "???");

                                var selectionsForConstant = pairGroups
                                    .Where(p => p.ConstantId == constantId)
                                    .ToList();

                                var totalForConstant = selectionsForConstant.Sum(p => p.Count);

                                foreach (var selection in selectionsForConstant)
                                {
                                    var selectedText = allOptions.GetValueOrDefault(selection.SelectedId, "???");
                                    var isCorrect = selection.SelectedId == correctCorrespondingId;
                                    var percent = totalForConstant > 0
                                        ? Math.Round((double)selection.Count / totalForConstant * 100, 1)
                                        : 0;

                                    pairStats.Add(new PairDetail
                                    {
                                        ConstantText = constantText,
                                        SelectedText = selectedText,
                                        CorrectText = correctText,
                                        Count = selection.Count,
                                        Percentage = percent,
                                        IsCorrect = isCorrect,
                                        FullPairText = $"{constantText} → {selectedText}"
                                    });
                                }
                            }

                            pairStats = new ObservableCollection<PairDetail>(
                                pairStats.OrderBy(p => p.ConstantText)
                                         .ThenByDescending(p => p.Count));
                        }
                    }
                    else if (question.QuestionTypeId == 2)
                    {
                        if (question.Options?.Any() == true)
                        {
                            foreach (var option in question.Options)
                            {
                                var optionCountCorrect = filteredAnswers.Where(p => p.IsCorrect == true).Count(a => a.OptionId == option.Id);
                                var optionPercent = totalAnswers > 0
                                    ? Math.Round((double)optionCountCorrect / totalAnswers * 100, 1)
                                    : 0;

                                var optdet = new OptionDetail
                                {
                                    QuestionOpt = option.Question,
                                    Text = option.TextAnswer,
                                    Count = optionCountCorrect,
                                    Percentage = optionPercent,
                                    IsCorrect = option.IsCorrect ?? false,
                                    BarWidth = optionPercent
                                };

                                var textAnswers = filteredAnswers
                                  .Where(a => !string.IsNullOrWhiteSpace(a.TextAnswer))
                                  .Select(a => a.TextAnswer.Trim())
                                  .ToList();

                                var correctTexts = question.Options?
                                    .Where(o => o.IsCorrect == true)
                                    .Select(o => o.TextAnswer?.ToLower().Trim() ?? "")
                                    .ToHashSet() ?? new HashSet<string>();

                                var wrongAnswers = textAnswers
                                    .Where(a => !correctTexts.Contains(a.ToLower().Trim()))
                                    .GroupBy(a => a.ToLower().Trim())
                                    .Select(g => new OtherAnswer
                                    {
                                        Text = g.First(),
                                        Count = g.Count(),
                                        Percentage = totalAnswers > 0 ? Math.Round((double)g.Count() / totalAnswers * 100, 1) : 0
                                    })
                                    .OrderByDescending(o => o.Count)
                                    .ToList();

                                optdet.OtherAnswers = new ObservableCollection<OtherAnswer>(wrongAnswers);

                                optionStats.Add(optdet);
                            }
                        }
                    }
                    else if (question.Options?.Any() == true)
                    {
                        foreach (var option in question.Options)
                        {
                            var optionCount = filteredAnswers.Count(a => a.OptionId == option.Id);
                            var optionPercent = totalAnswers > 0
                                ? Math.Round((double)optionCount / totalAnswers * 100, 1)
                                : 0;

                            var optdet = new OptionDetail
                            {
                                QuestionOpt = option.Question,
                                Text = option.TextAnswer,
                                Count = optionCount,
                                Percentage = optionPercent,
                                IsCorrect = option.IsCorrect ?? false,
                                BarWidth = optionPercent
                            };
                            optionStats.Add(optdet);
                        }
                    }

                    details.Add(new QuestionDetailStatistics
                    {
                        QuestionText = question.TestQuest,
                        CorrectCount = correctCount,
                        TotalCount = totalAnswers,
                        Options = optionStats,
                        Pairs = pairStats,
                        ChartData = optionStats.Select(o => (double)o.Count).ToArray(),
                        ChartLabels = optionStats.Select(o => o.Text).ToArray()
                    });
                }

                QuestionsWrong = wrongs;
                QuestionDetails = details;
            }
            catch (Exception ex)
            {
                CountQuest = 0;
                AverageScore = 0;
                MedianScore = 0;
                MinScore = 0;
                MaxScore = 0;
                ScoreDistributionData = Array.Empty<double>();
                ScoreDistributionLabels = Array.Empty<string>();
                QuestionsWrong = new ObservableCollection<QuestionSummary>();
                QuestionDetails = new ObservableCollection<QuestionDetailStatistics>();

                _dialogService.ShowMessage($"Ошибка загрузки статистики: {ex.Message}", "Ошибка");
            }
        }

    }

    public class QuestionSummary
    {
        public string QuestionText { get; set; }
        public int CorrectCount { get; set; }
        public int TotalCount { get; set; }
        public string RatioText => $"{CorrectCount}/{TotalCount}";
    }

    public class QuestionDetailStatistics
    {
        public string QuestionText { get; set; }
        public int CorrectCount { get; set; }
        public int QuestionTypeId { get; set; }
        public int TotalCount { get; set; }
        public string CorrectRatioText => $"Верных ответов: {CorrectCount} из {TotalCount}";
        public ObservableCollection<PairDetail> Pairs { get; set; } = new();
        public ObservableCollection<OptionDetail> Options { get; set; } = new();
        public double[] ChartData { get; set; }
        public string[] ChartLabels { get; set; }
    }

    public class OptionDetail
    {
        public string Text { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
        public bool IsCorrect { get; set; }
        public double BarWidth { get; set; }
        public string PercentageText => $"{Percentage:F1} %";
        public string CountText => $"{Count} ({Percentage:F0} %)";
        public Question QuestionOpt { get; set; }
        public ObservableCollection<OtherAnswer> OtherAnswers { get; set; } = new();
    }

    public class OtherAnswer
    {
        public string Text { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
        public string CountText => $"{Count} ({Percentage:F0}%)";
    }

    public class PairDetail
    {
        public string ConstantText { get; set; }
        public string SelectedText { get; set; }
        public string CorrectText { get; set; }
        public string FullPairText { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
        public bool IsCorrect { get; set; }
        public string CountText => $"{Count} ({Percentage:F0}%)";
        public string CorrectIndicator => IsCorrect ? "✓" : "✗";
    }
}